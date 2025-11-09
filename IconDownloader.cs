using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordActivityMonitor
{
    public class IconDownloader
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        public static async Task DownloadAllIconsForDiscord(string outputFolder)
        {
            Console.WriteLine("📥 Downloading icons for Discord upload...\n");
            
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            
            var icons = new Dictionary<string, string>
            {
                // Video & Streaming
                { "youtube_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/youtube_icon.png" },
                { "netflix_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/netflix_icon.png" },
                { "twitch_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/twitch_icon.png" },
                
                // Music
                { "spotify_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/spotify_icon.png" },
                { "soundcloud_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/soundcloud_icon.png" },
                
                // Social Media
                { "facebook_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/facebook_icon.png" },
                { "twitter_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/twitter_icon.png" },
                { "instagram_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/instagram_icon.png" },
                { "reddit_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/reddit_icon.png" },
                { "linkedin_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/linkedin_icon.png" },
                { "discord_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/discord_icon.png" },
                
                // Development
                { "github_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/github_icon.png" },
                { "stackoverflow_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/stackoverflow_icon.png" },
                { "vscode_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/vscode_icon.png" },
                
                // Shopping & Info
                { "amazon_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/amazon_icon.png" },
                { "wikipedia_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/wikipedia_icon.png" },
                
                // Web Languages (Frontend)
                { "javascript_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/javascript_icon.png" },
                { "typescript_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/typescript_icon.png" },
                { "react_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/react_icon.png" },
                { "html_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/html_icon.png" },
                { "css_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/css_icon.png" },
                { "sass_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/sass_icon.png" },
                { "vue_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/vue_icon.png" },
                { "svelte_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/svelte_icon.png" },
                
                // Backend Languages
                { "python_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/python_icon.png" },
                { "java_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/java_icon.png" },
                { "csharp_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/csharp_icon.png" },
                { "cpp_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/cpp_icon.png" },
                { "c_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/c_icon.png" },
                { "go_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/go_icon.png" },
                { "rust_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/rust_icon.png" },
                { "php_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/php_icon.png" },
                { "ruby_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/ruby_icon.png" },
                { "swift_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/swift_icon.png" },
                { "kotlin_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/kotlin_icon.png" },
                { "scala_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/scala_icon.png" },
                
                // Scripting & Shell
                { "bash_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/bash_icon.png" },
                { "powershell_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/powershell_icon.png" },
                { "cmd_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/cmd_icon.png" },
                
                // Data & Config
                { "json_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/json_icon.png" },
                { "yaml_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/yaml_icon.png" },
                { "xml_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/xml_icon.png" },
                { "toml_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/toml_icon.png" },
                { "config_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/config_icon.png" },
                
                // Database
                { "sql_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/sql_icon.png" },
                
                // Docs
                { "markdown_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/markdown_icon.png" },
                { "text_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/text_icon.png" },
                
                // Browsers & Apps
                { "chrome_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/chrome_icon.png" },
                { "firefox_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/firefox_icon.png" },
                { "edge_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/edge_icon.png" },
                { "browser_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/browser_icon.png" },
                { "notepad_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/notepad_icon.png" },
                { "explorer_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/explorer_icon.png" },
                { "default_icon", "https://github.com/AbdullahArefin-r/DiscordRPC/raw/main/icons/default_icon.png" },
            };
            
            int success = 0;
            int failed = 0;
            
            foreach (var icon in icons)
            {
                try
                {
                    Console.Write($"Downloading {icon.Key}... ");
                    var response = await httpClient.GetAsync(icon.Value);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        var filePath = Path.Combine(outputFolder, $"{icon.Key}.png");
                        await File.WriteAllBytesAsync(filePath, bytes);
                        Console.WriteLine("✅");
                        success++;
                    }
                    else
                    {
                        Console.WriteLine("❌ Failed");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    failed++;
                }
                
                await Task.Delay(500); // Don't spam the server
            }
            
            Console.WriteLine($"\n✅ Downloaded: {success}");
            Console.WriteLine($"❌ Failed: {failed}");
            Console.WriteLine($"\n📁 Icons saved to: {outputFolder}");
            Console.WriteLine("\n📝 Next steps:");
            Console.WriteLine("1. Go to https://discord.com/developers/applications/1435943653448220814");
            Console.WriteLine("2. Click 'Rich Presence' → 'Art Assets'");
            Console.WriteLine("3. Upload all PNG files from the folder above");
            Console.WriteLine("4. Use the filename (without .png) as the icon name in Discord");
            Console.WriteLine("5. Save changes and restart the app!");
        }
    }
}
