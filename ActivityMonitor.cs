using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Drawing;
using DiscordRPC;

namespace DiscordActivityMonitor
{
    public class ActivityMonitor
    {
        private readonly DiscordRpcClient client;
        private readonly Action<string, Color>? logCallback;
        private readonly Action<Image?>? iconCallback;
        private string lastActivity = "";
        private string lastIconIdentifier = "";
        private DateTime startTime = DateTime.UtcNow;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public ActivityMonitor(DiscordRpcClient rpcClient, Action<string, Color>? callback = null, Action<Image?>? iconCallback = null)
        {
            client = rpcClient;
            logCallback = callback;
            this.iconCallback = iconCallback;
        }

        public void Update()
        {
            try
            {
                var activity = GetCurrentActivity();
                
                if (activity != lastActivity)
                {
                    UpdateDiscordPresence(activity);
                    lastActivity = activity;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating activity: {ex.Message}");
            }
        }

        private string GetCurrentActivity()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);

            StringBuilder windowTitle = new StringBuilder(256);
            GetWindowText(hwnd, windowTitle, 256);

            try
            {
                Process process = Process.GetProcessById((int)processId);
                string processName = process.ProcessName.ToLower();
                string title = windowTitle.ToString();

                return $"{processName}|||{title}";
            }
            catch
            {
                return "idle|||";
            }
        }

        private void UpdateDiscordPresence(string activity)
        {
            var parts = activity.Split("|||");
            string processName = parts[0];
            string windowTitle = parts.Length > 1 ? parts[1] : "";

            var presence = new RichPresence();
            
            // Detect different applications and set appropriate status
            if (processName.Contains("chrome") || processName.Contains("firefox") || 
                processName.Contains("edge") || processName.Contains("brave") || 
                processName.Contains("msedge") || processName.Contains("opera"))
            {
                HandleBrowserActivity(windowTitle, processName, ref presence);
            }
            else if (processName.Contains("spotify"))
            {
                HandleSpotifyActivity(windowTitle, ref presence);
            }
            else if (processName.Contains("code") || processName.Contains("vscode"))
            {
                HandleVSCodeActivity(windowTitle, ref presence);
            }
            else if (processName.Contains("discord"))
            {
                presence.Details = "Using Discord";
                presence.State = "Chatting";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "discord_icon",
                    LargeImageText = "Discord"
                };
            }
            else if (processName.Contains("notepad"))
            {
                presence.Details = "Editing in Notepad";
                presence.State = TruncateString(windowTitle, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "notepad_icon",
                    LargeImageText = "Notepad"
                };
            }
            else if (processName.Contains("explorer"))
            {
                presence.Details = "File Explorer";
                presence.State = TruncateString(windowTitle, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "explorer_icon",
                    LargeImageText = "File Explorer"
                };
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                presence.Details = $"Using {CapitalizeFirst(processName)}";
                presence.State = TruncateString(windowTitle, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "default_icon",
                    LargeImageText = CapitalizeFirst(processName)
                };
            }
            else
            {
                presence.Details = "On Desktop";
                presence.State = "Idle";
            }

            // Only update timestamp when activity changes
            if (activity != lastActivity)
            {
                startTime = DateTime.UtcNow;
            }

            presence.Timestamps = new Timestamps()
            {
                Start = startTime
            };

            try
            {
                client.SetPresence(presence);
                string logMessage = $"{presence.Details} - {presence.State}";
                
                // Log the icon being used for debugging
                if (presence.Assets != null && !string.IsNullOrEmpty(presence.Assets.LargeImageKey))
                {
                    logMessage += $" [Icon: {presence.Assets.LargeImageKey}]";
                }
                
                logCallback?.Invoke(logMessage, Color.FromArgb(185, 187, 190));
                
                // Update icon asynchronously
                _ = UpdateIconAsync(processName, windowTitle);
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"Failed to update presence: {ex.Message}", Color.FromArgb(237, 66, 69));
            }
        }

        private async Task UpdateIconAsync(string processName, string windowTitle)
        {
            try
            {
                string iconIdentifier = processName;
                
                // Check if we need to update the icon
                if (iconIdentifier == lastIconIdentifier)
                    return;
                
                lastIconIdentifier = iconIdentifier;
                
                Image? icon = null;
                
                logCallback?.Invoke($"🔍 Fetching icon for: {processName}", Color.FromArgb(250, 166, 26));
                
                // Try to get favicon for browsers
                if (processName.Contains("chrome") || processName.Contains("firefox") || 
                    processName.Contains("edge") || processName.Contains("msedge") || 
                    processName.Contains("brave") || processName.Contains("opera"))
                {
                    // Try to extract URL and get favicon
                    if (windowTitle.Contains("YouTube") || windowTitle.Contains("youtube"))
                    {
                        logCallback?.Invoke("📥 Downloading YouTube icon...", Color.FromArgb(250, 166, 26));
                        icon = await IconManager.GetIconAsync("youtube");
                    }
                    else if (windowTitle.Contains("Spotify") || windowTitle.Contains("spotify"))
                    {
                        logCallback?.Invoke("📥 Downloading Spotify icon...", Color.FromArgb(250, 166, 26));
                        icon = await IconManager.GetIconAsync("spotify");
                    }
                    else if (windowTitle.Contains("Netflix") || windowTitle.Contains("netflix"))
                    {
                        logCallback?.Invoke("📥 Downloading Netflix icon...", Color.FromArgb(250, 166, 26));
                        icon = await IconManager.GetIconAsync("netflix");
                    }
                    else if (windowTitle.Contains("Twitch") || windowTitle.Contains("twitch"))
                    {
                        logCallback?.Invoke("📥 Downloading Twitch icon...", Color.FromArgb(250, 166, 26));
                        icon = await IconManager.GetIconAsync("twitch");
                    }
                    else if (windowTitle.Contains("SoundCloud") || windowTitle.Contains("soundcloud"))
                    {
                        logCallback?.Invoke("📥 Downloading SoundCloud icon...", Color.FromArgb(250, 166, 26));
                        icon = await IconManager.GetIconAsync("soundcloud");
                    }
                    else
                    {
                        // Try to get generic browser icon or favicon
                        logCallback?.Invoke($"📥 Downloading {processName} icon...", Color.FromArgb(250, 166, 26));
                        icon = await IconManager.GetIconAsync(processName);
                    }
                }
                else
                {
                    // Try to get icon from executable or identifier
                    logCallback?.Invoke($"📥 Getting icon from executable: {processName}", Color.FromArgb(250, 166, 26));
                    icon = IconManager.GetIconFromExecutable(processName);
                    if (icon == null)
                    {
                        icon = await IconManager.GetIconAsync(processName);
                    }
                }
                
                if (icon != null)
                {
                    logCallback?.Invoke($"✅ Icon loaded successfully!", Color.FromArgb(67, 181, 129));
                    // Update UI with the icon
                    iconCallback?.Invoke(icon);
                }
                else
                {
                    logCallback?.Invoke($"⚠️ No icon found for {processName}", Color.FromArgb(250, 166, 26));
                }
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"❌ Icon error: {ex.Message}", Color.FromArgb(237, 66, 69));
            }
        }

        private void HandleBrowserActivity(string title, string browser, ref RichPresence presence)
        {
            string browserName = CapitalizeFirst(browser.Replace("applicationframehost", "Edge").Replace("msedge", "Edge"));
            
            // Detect YouTube
            if (title.Contains("YouTube", StringComparison.OrdinalIgnoreCase) || 
                title.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
            {
                string videoTitle = ExtractYouTubeTitle(title);
                
                // Check if watching or just browsing
                if (title.Contains(" - YouTube"))
                {
                    presence.Details = "Watching YouTube";
                    presence.State = TruncateString(videoTitle, 128);
                }
                else
                {
                    presence.Details = "Browsing YouTube";
                    presence.State = "Looking for videos";
                }
                
                presence.Assets = new Assets()
                {
                    LargeImageKey = "youtube_icon",
                    LargeImageText = "YouTube"
                };
                
                // Add small browser icon if available
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect music streaming services
            else if (title.Contains("Spotify", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("open.spotify.com", StringComparison.OrdinalIgnoreCase))
            {
                string songInfo = ExtractSpotifyInfo(title);
                presence.Details = "Listening to Spotify";
                presence.State = TruncateString(songInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "spotify_icon",
                    LargeImageText = "Spotify Web Player"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            else if (title.Contains("SoundCloud", StringComparison.OrdinalIgnoreCase))
            {
                string songInfo = ExtractSoundCloudInfo(title);
                presence.Details = "Listening on SoundCloud";
                presence.State = TruncateString(songInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "soundcloud_icon",
                    LargeImageText = "SoundCloud"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Netflix
            else if (title.Contains("Netflix", StringComparison.OrdinalIgnoreCase))
            {
                string showInfo = ExtractNetflixInfo(title);
                presence.Details = "Watching Netflix";
                presence.State = TruncateString(showInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "netflix_icon",
                    LargeImageText = "Netflix"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Twitch
            else if (title.Contains("Twitch", StringComparison.OrdinalIgnoreCase))
            {
                string streamInfo = ExtractTwitchInfo(title);
                presence.Details = "Watching Twitch";
                presence.State = TruncateString(streamInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "twitch_icon",
                    LargeImageText = "Twitch"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect GitHub
            else if (title.Contains("GitHub", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("github.com", StringComparison.OrdinalIgnoreCase))
            {
                string repoInfo = ExtractGitHubInfo(title);
                presence.Details = "Browsing GitHub";
                presence.State = TruncateString(repoInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "github_icon",
                    LargeImageText = "GitHub"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Facebook
            else if (title.Contains("Facebook", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("facebook.com", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Browsing Facebook";
                presence.State = "Scrolling the feed";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "facebook_icon",
                    LargeImageText = "Facebook"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Twitter/X
            else if (title.Contains("Twitter", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("twitter.com", StringComparison.OrdinalIgnoreCase) ||
                     title.Contains("x.com", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Browsing X (Twitter)";
                presence.State = "Scrolling timeline";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "twitter_icon",
                    LargeImageText = "X (Twitter)"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Instagram
            else if (title.Contains("Instagram", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("instagram.com", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Browsing Instagram";
                presence.State = "Scrolling posts";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "instagram_icon",
                    LargeImageText = "Instagram"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Reddit
            else if (title.Contains("Reddit", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("reddit.com", StringComparison.OrdinalIgnoreCase))
            {
                string subredditInfo = ExtractRedditInfo(title);
                presence.Details = "Browsing Reddit";
                presence.State = TruncateString(subredditInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "reddit_icon",
                    LargeImageText = "Reddit"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Stack Overflow
            else if (title.Contains("Stack Overflow", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("stackoverflow.com", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Browsing Stack Overflow";
                presence.State = "Searching for answers";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "stackoverflow_icon",
                    LargeImageText = "Stack Overflow"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect LinkedIn
            else if (title.Contains("LinkedIn", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Browsing LinkedIn";
                presence.State = "Professional networking";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "linkedin_icon",
                    LargeImageText = "LinkedIn"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Amazon
            else if (title.Contains("Amazon", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("amazon.com", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Browsing Amazon";
                presence.State = "Shopping online";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "amazon_icon",
                    LargeImageText = "Amazon"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Wikipedia
            else if (title.Contains("Wikipedia", StringComparison.OrdinalIgnoreCase) || 
                     title.Contains("wikipedia.org", StringComparison.OrdinalIgnoreCase))
            {
                presence.Details = "Reading Wikipedia";
                presence.State = "Learning something new";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "wikipedia_icon",
                    LargeImageText = "Wikipedia"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Detect Twitch
            else if (title.Contains("Twitch", StringComparison.OrdinalIgnoreCase))
            {
                string streamerInfo = ExtractTwitchInfo(title);
                presence.Details = "Watching Twitch";
                presence.State = TruncateString(streamerInfo, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "twitch_icon",
                    LargeImageText = "Twitch"
                };
                
                string browserIcon = GetBrowserIconName(browser);
                if (!string.IsNullOrEmpty(browserIcon))
                {
                    presence.Assets.SmallImageKey = browserIcon;
                    presence.Assets.SmallImageText = browserName;
                }
            }
            // Generic browser activity
            else if (!string.IsNullOrEmpty(title))
            {
                string siteName = ExtractWebsiteName(title);
                presence.Details = $"Browsing {siteName}";
                presence.State = TruncateString(title, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = browser.Contains("chrome") ? "chrome_icon" : browser.Contains("firefox") ? "firefox_icon" : "browser_icon",
                    LargeImageText = browserName
                };
            }
            else
            {
                presence.Details = $"Using {browserName}";
                presence.State = "Browsing the web";
                presence.Assets = new Assets()
                {
                    LargeImageKey = browser.Contains("chrome") ? "chrome_icon" : browser.Contains("firefox") ? "firefox_icon" : "browser_icon",
                    LargeImageText = browserName
                };
            }
        }

        private string GetBrowserIconName(string browser)
        {
            if (browser.Contains("chrome")) return "chrome_icon";
            if (browser.Contains("firefox")) return "firefox_icon";
            if (browser.Contains("edge") || browser.Contains("msedge")) return "edge_icon";
            if (browser.Contains("brave")) return "brave_icon";
            if (browser.Contains("opera")) return "opera_icon";
            return "browser_icon";
        }

        private void HandleSpotifyActivity(string title, ref RichPresence presence)
        {
            if (!string.IsNullOrEmpty(title) && !title.Equals("Spotify", StringComparison.OrdinalIgnoreCase) && 
                !title.Equals("Spotify Free", StringComparison.OrdinalIgnoreCase) &&
                !title.Equals("Spotify Premium", StringComparison.OrdinalIgnoreCase))
            {
                // Spotify desktop shows "Artist - Song" format
                presence.Details = "Listening to Spotify";
                presence.State = TruncateString(title, 128);
                presence.Assets = new Assets()
                {
                    LargeImageKey = "spotify_icon",
                    LargeImageText = "Spotify Desktop"
                };
            }
            else
            {
                presence.Details = "Using Spotify";
                presence.State = "Browsing music";
                presence.Assets = new Assets()
                {
                    LargeImageKey = "spotify_icon",
                    LargeImageText = "Spotify"
                };
            }
        }

        private void HandleVSCodeActivity(string title, ref RichPresence presence)
        {
            string fileName = "";
            string fileExtension = "";
            
            if (!string.IsNullOrEmpty(title))
            {
                fileName = ExtractFileName(title);
                fileExtension = GetFileExtension(fileName);
            }
            
            // Detect language from file extension
            var languageInfo = GetLanguageFromExtension(fileExtension);
            
            presence.Details = $"Coding in {languageInfo.Name}";
            presence.State = !string.IsNullOrEmpty(fileName) ? TruncateString(fileName, 128) : "Working on a project";
            
            presence.Assets = new Assets()
            {
                LargeImageKey = languageInfo.IconKey,
                LargeImageText = languageInfo.Name,
                SmallImageKey = "vscode_icon",
                SmallImageText = "Visual Studio Code"
            };
        }
        
        private string GetFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "";
            
            int dotIndex = fileName.LastIndexOf('.');
            if (dotIndex > 0 && dotIndex < fileName.Length - 1)
            {
                return fileName.Substring(dotIndex + 1).ToLower();
            }
            return "";
        }
        
        private (string Name, string IconKey) GetLanguageFromExtension(string extension)
        {
            return extension switch
            {
                // Web Development
                "js" => ("JavaScript", "javascript_icon"),
                "jsx" => ("React JSX", "react_icon"),
                "ts" => ("TypeScript", "typescript_icon"),
                "tsx" => ("React TSX", "react_icon"),
                "html" => ("HTML", "html_icon"),
                "htm" => ("HTML", "html_icon"),
                "css" => ("CSS", "css_icon"),
                "scss" => ("SCSS", "sass_icon"),
                "sass" => ("Sass", "sass_icon"),
                "less" => ("Less", "css_icon"),
                "vue" => ("Vue.js", "vue_icon"),
                "svelte" => ("Svelte", "svelte_icon"),
                
                // Backend & Server
                "py" => ("Python", "python_icon"),
                "java" => ("Java", "java_icon"),
                "cs" => ("C#", "csharp_icon"),
                "cpp" => ("C++", "cpp_icon"),
                "c" => ("C", "c_icon"),
                "h" => ("C/C++ Header", "cpp_icon"),
                "hpp" => ("C++ Header", "cpp_icon"),
                "go" => ("Go", "go_icon"),
                "rs" => ("Rust", "rust_icon"),
                "php" => ("PHP", "php_icon"),
                "rb" => ("Ruby", "ruby_icon"),
                "swift" => ("Swift", "swift_icon"),
                "kt" => ("Kotlin", "kotlin_icon"),
                "scala" => ("Scala", "scala_icon"),
                
                // Scripting & Shell
                "sh" => ("Shell Script", "bash_icon"),
                "bash" => ("Bash", "bash_icon"),
                "zsh" => ("Zsh", "bash_icon"),
                "ps1" => ("PowerShell", "powershell_icon"),
                "bat" => ("Batch Script", "cmd_icon"),
                "cmd" => ("Command Script", "cmd_icon"),
                
                // Data & Config
                "json" => ("JSON", "json_icon"),
                "xml" => ("XML", "xml_icon"),
                "yaml" => ("YAML", "yaml_icon"),
                "yml" => ("YAML", "yaml_icon"),
                "toml" => ("TOML", "toml_icon"),
                "ini" => ("INI Config", "config_icon"),
                "conf" => ("Config", "config_icon"),
                
                // Database
                "sql" => ("SQL", "sql_icon"),
                
                // Markdown & Docs
                "md" => ("Markdown", "markdown_icon"),
                "mdx" => ("MDX", "markdown_icon"),
                "txt" => ("Text", "text_icon"),
                "rst" => ("reStructuredText", "text_icon"),
                
                // Default fallback
                _ => ("VS Code", "vscode_icon")
            };
        }

        private string ExtractYouTubeTitle(string title)
        {
            // YouTube titles usually end with " - YouTube"
            int index = title.LastIndexOf(" - YouTube");
            if (index > 0)
            {
                return title.Substring(0, index).Trim();
            }
            return title;
        }

        private string ExtractWebsiteName(string title)
        {
            // Try to extract website name from common patterns
            string[] separators = { " - ", " | ", " — " };
            foreach (var sep in separators)
            {
                int index = title.LastIndexOf(sep);
                if (index > 0 && index < title.Length - sep.Length)
                {
                    return title.Substring(index + sep.Length).Trim();
                }
            }
            return "the web";
        }

        private string ExtractFileName(string title)
        {
            // VS Code titles usually have format "filename - folder - Visual Studio Code"
            int index = title.IndexOf(" - ");
            if (index > 0)
            {
                return title.Substring(0, index).Trim();
            }
            return title;
        }

        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private string ExtractSpotifyInfo(string title)
        {
            // Spotify web player format: "Song Name • Artist - Spotify"
            int spotifyIndex = title.LastIndexOf(" - Spotify");
            if (spotifyIndex > 0)
            {
                return title.Substring(0, spotifyIndex).Trim();
            }
            return title;
        }

        private string ExtractSoundCloudInfo(string title)
        {
            // SoundCloud format: "Song Name by Artist - SoundCloud"
            int soundcloudIndex = title.LastIndexOf(" - SoundCloud");
            if (soundcloudIndex > 0)
            {
                return title.Substring(0, soundcloudIndex).Trim();
            }
            return title;
        }

        private string ExtractNetflixInfo(string title)
        {
            // Netflix format: "Show Name - Netflix"
            int netflixIndex = title.LastIndexOf(" - Netflix");
            if (netflixIndex > 0)
            {
                return title.Substring(0, netflixIndex).Trim();
            }
            return title;
        }

        private string ExtractTwitchInfo(string title)
        {
            // Twitch format: "Streamer Name - Twitch"
            int twitchIndex = title.LastIndexOf(" - Twitch");
            if (twitchIndex > 0)
            {
                return title.Substring(0, twitchIndex).Trim();
            }
            return title;
        }

        private string ExtractGitHubInfo(string title)
        {
            // GitHub format: "Repository Name - GitHub"
            int githubIndex = title.LastIndexOf(" · GitHub");
            if (githubIndex > 0)
            {
                return title.Substring(0, githubIndex).Trim();
            }
            githubIndex = title.LastIndexOf(" - GitHub");
            if (githubIndex > 0)
            {
                return title.Substring(0, githubIndex).Trim();
            }
            return title;
        }

        private string ExtractRedditInfo(string title)
        {
            // Reddit format: "Post Title : Subreddit"
            if (title.Contains(" : r/"))
            {
                int subredditIndex = title.IndexOf(" : r/");
                return title.Substring(subredditIndex + 3).Trim();
            }
            return title;
        }
    }
}
