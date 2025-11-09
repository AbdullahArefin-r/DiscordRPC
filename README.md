# Discord Activity Monitor

A custom Discord Rich Presence application that monitors your PC activity and displays what you're doing in real-time.

## Features

- 🎯 **Automatic Activity Detection**: Monitors active applications
- 🌐 **Browser Integration**: Detects Chrome, Firefox, Edge, Brave
- 🎵 **Music Detection**: Shows what you're listening to (Spotify, YouTube Music, SoundCloud)
- 📺 **Video Detection**: Displays YouTube videos and Netflix shows you're watching
- 💻 **Development Tools**: Shows files you're editing in VS Code
- 🎨 **Custom Icons**: Displays appropriate icons for different activities

## Setup Instructions

### 1. Create Discord Application

1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application"
3. Give it a name (e.g., "Activity Monitor")
4. Go to "Rich Presence" → "Art Assets"
5. Upload icons with these names:
   - `youtube_icon` - YouTube logo
   - `spotify_icon` - Spotify logo
   - `netflix_icon` - Netflix logo
   - `browser_icon` - Browser logo
   - `vscode_icon` - VS Code logo
   - `discord_icon` - Discord logo
   - `soundcloud_icon` - SoundCloud logo
   - `default_icon` - Default/generic icon
6. Copy your "Application ID" (Client ID)

### 2. Configure the Application

### 3. Build and Run

#### Option 1: Using Visual Studio
1. Open the folder in Visual Studio
2. Press F5 to build and run

#### Option 2: Using Command Line
```powershell
dotnet restore
dotnet build
dotnet run
```

#### Option 3: Create Executable
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
The .exe file will be in: `bin\Release\net6.0\win-x64\publish\`

## How It Works

The application:
1. Monitors your active window every 5 seconds
2. Detects the application and window title
3. Parses the information to determine what you're doing
4. Updates your Discord Rich Presence with:
   - Activity details (e.g., "Watching YouTube")
   - Specific content (e.g., song name, video title)
   - Appropriate icon for the activity

## Supported Applications

- **Browsers**: Chrome, Firefox, Edge, Brave
  - YouTube videos
  - Spotify Web Player
  - SoundCloud
  - Netflix
  - Generic websites
- **Music**: Spotify desktop app
- **Development**: Visual Studio Code
- **Communication**: Discord
- Any other application (shows generic status)

## Customization

### Add More Applications

Edit `ActivityMonitor.cs` in the `UpdateDiscordPresence` method:

```csharp
else if (processName.Contains("yourapp"))
{
    presence.Details = "Using Your App";
    presence.State = windowTitle;
    presence.Assets = new Assets()
    {
        LargeImageKey = "yourapp_icon",
        LargeImageText = "Your App"
    };
}
```

### Change Update Interval

In `Program.cs`, modify the sleep duration:
```csharp
Thread.Sleep(5000); // 5 seconds (5000 milliseconds)
```

### Add Website Detection

In `HandleBrowserActivity` method, add more conditions:
```csharp
else if (title.Contains("Reddit", StringComparison.OrdinalIgnoreCase))
{
    presence.Details = "Browsing Reddit";
    presence.State = TruncateString(title, 128);
    presence.Assets = new Assets()
    {
        LargeImageKey = "reddit_icon",
        LargeImageText = "Reddit"
    };
}
```

## Privacy Note

This application only monitors your active window title and process name. It does NOT:
- Record your screen
- Send data to external servers (only Discord)
- Monitor keyboard/mouse activity
- Access file contents

## Troubleshooting

### "Client ID not found"
- Make sure you replaced `YOUR_CLIENT_ID_HERE` with your actual Discord Application ID

### "No presence showing"
- Ensure Discord is running
- Wait a few seconds after starting the app
- Check that your Discord privacy settings allow Rich Presence

### Icons not showing
- Upload icons to Discord Developer Portal under "Rich Presence" → "Art Assets"
- Use exact names as specified in the code
- Icons must be at least 512x512 pixels

## Auto-Start on Windows

To run automatically when Windows starts:

1. Build the executable (see Option 3 above)
2. Press `Win + R`, type `shell:startup`, press Enter
3. Create a shortcut to the .exe file in this folder

## Requirements

- Windows 10/11
- .NET 6.0 or later
- Discord application running

## License

Free to use and modify as needed!
