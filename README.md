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
- An Azure account (for Azure Functions hosting and Blob Storage)
- A [Spotify Developer](https://developer.spotify.com) application

## Configuration

Create a `playlists.json` file in `src/SpotifyPodcastPlaylist/Config/`:

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

## Authentication Setup

The function uses Spotify's OAuth2 Authorization Code flow. You need to obtain a refresh token once; the function then refreshes the access token automatically.

1. Register an application at [developer.spotify.com](https://developer.spotify.com).
2. Set a redirect URI in your app settings (e.g., `http://localhost:8888/callback`).
3. Authorize your app with the following scopes:
   - `playlist-modify-public`
   - `playlist-modify-private`
   - `playlist-read-private`
   - `user-read-playback-position`
4. Exchange the authorization code for tokens.
5. Store the refresh token in the `Spotify__RefreshToken` environment variable.

## Build and Run

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run locally (requires Azure Functions Core Tools)
cd src/SpotifyPodcastPlaylist
func start
```

For production, deploy to Azure Functions. The function runs on a timer trigger (every minute) and evaluates each playlist's cron schedule to determine if an update is due.

## License

Apache 2.0
