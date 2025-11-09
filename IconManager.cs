using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace DiscordActivityMonitor
{
    public class IconManager
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly Dictionary<string, Image> iconCache = new Dictionary<string, Image>();
        private static readonly string cacheFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DiscordActivityMonitor",
            "Icons"
        );

        static IconManager()
        {
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public static async Task<Image?> GetIconAsync(string identifier, string url = "")
        {
            // Check cache first
            if (iconCache.ContainsKey(identifier))
            {
                return iconCache[identifier];
            }

            // Check disk cache
            string cachePath = Path.Combine(cacheFolder, $"{SanitizeFileName(identifier)}.png");
            if (File.Exists(cachePath))
            {
                try
                {
                    var img = Image.FromFile(cachePath);
                    iconCache[identifier] = img;
                    return img;
                }
                catch { }
            }

            // Download icon
            Image? icon = null;
            
            if (!string.IsNullOrEmpty(url))
            {
                icon = await DownloadIconFromUrl(url);
            }
            else
            {
                icon = await DownloadIconForIdentifier(identifier);
            }

            if (icon != null)
            {
                try
                {
                    icon.Save(cachePath, System.Drawing.Imaging.ImageFormat.Png);
                    iconCache[identifier] = icon;
                }
                catch { }
            }

            return icon;
        }

        private static async Task<Image?> DownloadIconFromUrl(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes.Length > 0)
                    {
                        using (var ms = new MemoryStream(bytes))
                        {
                            var img = Image.FromStream(ms);
                            // Clone the image so we can dispose the stream
                            var clonedImg = new Bitmap(img);
                            return clonedImg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download icon from {url}: {ex.Message}");
            }
            return null;
        }

        private static async Task<Image?> DownloadIconForIdentifier(string identifier)
        {
            string lowerIdentifier = identifier.ToLower();

            // Try common icon sources
            string[] iconUrls = GetIconUrlsForIdentifier(lowerIdentifier);

            foreach (var url in iconUrls)
            {
                var icon = await DownloadIconFromUrl(url);
                if (icon != null)
                {
                    return icon;
                }
            }

            return null;
        }

        private static string[] GetIconUrlsForIdentifier(string identifier)
        {
            var urls = new List<string>();

            // Website favicons
            if (identifier.Contains("youtube"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=youtube.com&sz=128");
                urls.Add("https://www.youtube.com/favicon.ico");
            }
            else if (identifier.Contains("spotify"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=spotify.com&sz=128");
                urls.Add("https://www.spotify.com/favicon.ico");
            }
            else if (identifier.Contains("discord"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=discord.com&sz=128");
                urls.Add("https://discord.com/assets/07dca80a102d4149e9736d4b162cff6f.ico");
            }
            else if (identifier.Contains("netflix"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=netflix.com&sz=128");
                urls.Add("https://www.netflix.com/favicon.ico");
            }
            else if (identifier.Contains("twitch"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=twitch.tv&sz=128");
                urls.Add("https://www.twitch.tv/favicon.ico");
            }
            else if (identifier.Contains("soundcloud"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=soundcloud.com&sz=128");
                urls.Add("https://a-v2.sndcdn.com/assets/images/sc-icons/favicon-2cadd14bdb.ico");
            }
            else if (identifier.Contains("chrome"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=google.com&sz=128");
                urls.Add("https://www.google.com/chrome/static/images/favicons/favicon-96x96.png");
            }
            else if (identifier.Contains("firefox"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=mozilla.org&sz=128");
                urls.Add("https://www.mozilla.org/media/img/favicons/firefox/browser/favicon-196x196.png");
            }
            else if (identifier.Contains("edge") || identifier.Contains("msedge"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=microsoft.com&sz=128");
                urls.Add("https://www.microsoft.com/favicon.ico");
            }
            else if (identifier.Contains("vscode") || identifier.Contains("code"))
            {
                urls.Add("https://www.google.com/s2/favicons?domain=code.visualstudio.com&sz=128");
                urls.Add("https://code.visualstudio.com/favicon.ico");
            }
            else if (identifier.StartsWith("http"))
            {
                // Extract domain and get favicon
                try
                {
                    var uri = new Uri(identifier);
                    urls.Add($"https://www.google.com/s2/favicons?domain={uri.Host}&sz=128");
                    urls.Add($"{uri.Scheme}://{uri.Host}/favicon.ico");
                }
                catch { }
            }
            else
            {
                // Generic - try Google's favicon service with the identifier as domain
                urls.Add($"https://www.google.com/s2/favicons?domain={identifier}.com&sz=128");
            }

            return urls.ToArray();
        }

        public static Image? GetIconFromExecutable(string processName)
        {
            try
            {
                // Try to find the executable path
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    var mainModule = processes[0].MainModule;
                    if (mainModule != null)
                    {
                        string exePath = mainModule.FileName;
                        Icon? icon = Icon.ExtractAssociatedIcon(exePath);
                        if (icon != null)
                        {
                            return icon.ToBitmap();
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static async Task<Image?> GetFaviconFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                string domain = uri.Host;
                
                // Try multiple favicon sources
                string[] faviconUrls = new[]
                {
                    $"{uri.Scheme}://{domain}/favicon.ico",
                    $"https://www.google.com/s2/favicons?domain={domain}&sz=128",
                    $"https://icons.duckduckgo.com/ip3/{domain}.ico",
                    $"{uri.Scheme}://{domain}/apple-touch-icon.png",
                    $"{uri.Scheme}://{domain}/apple-touch-icon-precomposed.png"
                };

                foreach (var faviconUrl in faviconUrls)
                {
                    var icon = await DownloadIconFromUrl(faviconUrl);
                    if (icon != null && icon.Width > 1 && icon.Height > 1)
                    {
                        return icon;
                    }
                }
            }
            catch { }
            return null;
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public static void ClearCache()
        {
            try
            {
                iconCache.Clear();
                if (Directory.Exists(cacheFolder))
                {
                    Directory.Delete(cacheFolder, true);
                    Directory.CreateDirectory(cacheFolder);
                }
            }
            catch { }
        }
    }
}
