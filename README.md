# Spotify Podcast Playlist

An Azure Function that automatically builds and maintains Spotify playlists from your podcast subscriptions. You configure which podcasts to include, how many episodes from each, and their priority — the function runs on a schedule and rebuilds the playlist so higher-priority shows always come first.

## Features

- **Priority-ordered playback** — Episodes from higher-priority podcasts (lower number) appear before lower-priority ones. Same-priority podcasts are interleaved round-robin.
- **Per-podcast episode caps** — Control how many episodes each show contributes to the playlist.
- **Configurable episode ordering** — Choose oldest-first (for serialized shows) or newest-first (for news/topical shows) per podcast.
- **Finished episode exclusion** — Episodes you've already listened to are automatically removed.
- **Lookback periods** — Optionally ignore episodes older than a specified number of days.
- **Title filtering** — Include or exclude episodes by regex match on title (e.g., skip bonus episodes).
- **Per-playlist schedules** — Each playlist updates on its own cron schedule.
- **Multiple playlists** — Define as many playlists as you want, each with its own set of podcasts.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local) (v4) for local development
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local Azure Storage emulation (or a real Azure Storage account)
- A [Spotify Developer](https://developer.spotify.com) application

## Local Development Setup

### 1. Create a Spotify application

1. Go to [developer.spotify.com](https://developer.spotify.com) and create a new application.
2. In your app settings, enable **Web API** under "APIs used".
3. Add a redirect URI: `http://127.0.0.1:5000/callback`.
4. If the app is in Development mode, add your Spotify account under "User Management".
5. Note your **Client ID** and **Client Secret** from the app settings.

### 2. Obtain a refresh token

A helper tool is included to run the OAuth2 Authorization Code flow:

```bash
dotnet run --project tools/AuthHelper
```

This will prompt for your Client ID and Client Secret, open a browser for authorization, and print a refresh token. If the browser redirects to a page that fails to load, copy the full URL from your browser's address bar and paste it into the console — the tool will extract the authorization code from it.

### 3. Configure local settings

Create `src/SpotifyPodcastPlaylist/local.settings.json` (this file is gitignored):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "Spotify__ClientId": "your-client-id",
    "Spotify__ClientSecret": "your-client-secret",
    "Spotify__RefreshToken": "your-refresh-token"
  }
}
```

Set `AzureWebJobsStorage` to `UseDevelopmentStorage=true` if running Azurite locally, or use a real Azure Storage connection string.

### 4. Start Azurite (if using local storage emulation)

```bash
azurite --silent
```

Or if installed as a VS Code extension, start it from the command palette.

### 5. Configure your playlists

Edit `src/SpotifyPodcastPlaylist/Config/playlists.json` with your podcast configuration (see Configuration section below).

To find Spotify IDs: open a podcast or playlist in Spotify, click "Share" > "Copy link". The ID is the string after the last `/` and before the `?` (e.g., `https://open.spotify.com/show/SHOW_ID_HERE?si=...`).

### 6. Run

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run the function locally
cd src/SpotifyPodcastPlaylist
func start
```

The function triggers every minute and evaluates each playlist's cron schedule to decide whether an update is due.

## Configuration

Edit the `playlists.json` file in `src/SpotifyPodcastPlaylist/Config/`:

```json
{
  "playlists": [
    {
      "playlistId": "your-spotify-playlist-id",
      "schedule": "0 * * * *",
      "podcasts": [
        {
          "showId": "spotify-show-id",
          "name": "My Favorite Podcast",
          "priority": 1,
          "maxEpisodes": 10,
          "episodeOrder": "oldestFirst",
          "maxLookbackDays": 30,
          "titleExclude": "Bonus|Trailer"
        },
        {
          "showId": "another-show-id",
          "name": "Daily News Podcast",
          "priority": 3,
          "maxEpisodes": 5,
          "episodeOrder": "newestFirst"
        }
      ]
    }
  ]
}
```

### Playlist fields

| Field | Required | Description |
|-------|----------|-------------|
| `playlistId` | Yes | The Spotify playlist ID to write to |
| `schedule` | Yes | Cron expression (5-field: minute, hour, day-of-month, month, day-of-week) |
| `podcasts` | Yes | At least one podcast entry |

### Podcast fields

| Field | Required | Default | Description |
|-------|----------|---------|-------------|
| `showId` | Yes | — | Spotify show ID |
| `name` | Yes | — | Display name (used in logs) |
| `priority` | Yes | — | Integer 1-10. Lower = played first. Same-priority shows are interleaved. |
| `maxEpisodes` | Yes | — | Maximum episodes to include from this show |
| `episodeOrder` | No | `oldestFirst` | `"oldestFirst"` or `"newestFirst"` |
| `maxLookbackDays` | No | — | Ignore episodes published more than this many days ago |
| `titleInclude` | No | — | Regex — only episodes with matching titles are kept |
| `titleExclude` | No | — | Regex — episodes with matching titles are removed |

## Environment Variables

| Variable | Description |
|----------|-------------|
| `Spotify__ClientId` | Your Spotify application's client ID |
| `Spotify__ClientSecret` | Your Spotify application's client secret |
| `Spotify__RefreshToken` | OAuth2 refresh token (see Authentication Setup below) |
| `AzureWebJobsStorage` | Azure Storage connection string (used for schedule tracking) |

For local development, set these in `src/SpotifyPodcastPlaylist/local.settings.json`.

## Authentication

The function uses Spotify's OAuth2 Authorization Code flow with the following scopes:

- `playlist-modify-public`
- `playlist-modify-private`
- `playlist-read-private`
- `user-read-playback-position`

You obtain a refresh token once (see step 2 in Local Development Setup above); the function automatically refreshes the access token at runtime.

## Production Deployment

Deploy to Azure Functions. Set the environment variables listed above in your Function App's application settings. The function runs on a timer trigger (every minute) and evaluates each playlist's cron schedule to determine if an update is due.

## License

Apache 2.0
