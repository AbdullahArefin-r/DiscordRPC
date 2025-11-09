# How to Add Icons to Discord Rich Presence

Discord Rich Presence requires you to upload icons to your Discord Application in advance. Here's how:

## Step 1: Go to Discord Developer Portal

1. Visit: https://discord.com/developers/applications/1435943653448220814
2. Select your application

## Step 2: Upload Art Assets

1. Click on **"Rich Presence"** in the left sidebar
2. Click on **"Art Assets"**
3. Click **"Add Image(s)"**

## Step 3: Upload These Icons

Download and upload icons with these **exact names**:

### Required Icons:

| Icon Name | What it's for | Download From |
|-----------|--------------|---------------|
| `youtube` | YouTube videos | https://www.youtube.com/favicon.ico |
| `spotify` | Spotify music | https://www.spotify.com/favicon.ico |
| `discord` | Discord app | https://discord.com/assets/favicon.ico |
| `netflix` | Netflix shows | https://www.netflix.com/favicon.ico |
| `twitch` | Twitch streams | https://www.twitch.tv/favicon.ico |
| `soundcloud` | SoundCloud music | https://soundcloud.com/favicon.ico |
| `chrome` | Google Chrome | https://www.google.com/chrome/static/images/favicons/favicon.ico |
| `firefox` | Firefox browser | https://www.mozilla.org/favicon.ico |
| `browser` | Generic browser | Any browser icon |
| `vscode` | VS Code | https://code.visualstudio.com/favicon.ico |
| `notepad` | Notepad | Windows notepad icon |
| `explorer` | File Explorer | Windows folder icon |
| `default` | Any other app | Generic app icon |

### How to Download Icons:

**Option 1: Manual Download**
1. Right-click each URL above
2. Save image as PNG/ICO
3. Upload to Discord

**Option 2: Use Google's Favicon Service**
- YouTube: `https://www.google.com/s2/favicons?domain=youtube.com&sz=512`
- Spotify: `https://www.google.com/s2/favicons?domain=spotify.com&sz=512`
- Discord: `https://www.google.com/s2/favicons?domain=discord.com&sz=512`
- Netflix: `https://www.google.com/s2/favicons?domain=netflix.com&sz=512`
- Twitch: `https://www.google.com/s2/favicons?domain=twitch.tv&sz=512`
- Chrome: `https://www.google.com/s2/favicons?domain=google.com&sz=512`
- Firefox: `https://www.google.com/s2/favicons?domain=mozilla.org&sz=512`
- VS Code: `https://www.google.com/s2/favicons?domain=code.visualstudio.com&sz=512`

**Option 3: Find High-Quality Icons**
- Search "YouTube logo PNG" on Google Images
- Use https://icons8.com
- Use https://www.flaticon.com

### Important Notes:

✅ **Image Requirements:**
- Minimum size: 512x512 pixels
- Maximum size: 1024x1024 pixels
- Format: PNG, JPG, or GIF
- File size: Under 10 MB

✅ **Icon Names Must Match Exactly:**
- Use lowercase names
- No spaces or special characters
- Must match the names in the code

## Step 4: Save Changes

After uploading all icons:
1. Click **"Save Changes"**
2. Wait a few minutes for Discord to process them
3. Restart your Discord Activity Monitor app

## Step 5: Test

1. Run `DiscordActivityMonitor.exe`
2. Open YouTube in Chrome
3. Check Discord - you should see YouTube icon!

## Quick Icon Pack

I'll create a helper to download all icons at once. Check the Icons folder after running the app:
`C:\Users\WALTON\AppData\Roaming\DiscordActivityMonitor\Icons\`

You can upload those directly to Discord!

---

## Why Can't We Use Dynamic Icons?

Discord Rich Presence is designed for security - it only allows icons that are pre-approved and uploaded to your application. This prevents:
- Malicious image URLs
- Privacy leaks
- Inappropriate content

The icons must be uploaded to Discord's CDN first, then referenced by name in the code.
