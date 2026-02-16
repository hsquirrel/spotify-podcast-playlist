# Product Requirements Document — Spotify Podcast Playlist

## 1. Overview

### Problem

Spotify provides no native way to prioritize, cap, and interleave podcast episodes across multiple shows into a single playlist. Listeners who follow many podcasts end up with a flat, chronological queue that buries episodes from shows they care about most.

### Solution

An automated Azure Function that maintains a Spotify playlist with priority-weighted episode ordering. Users configure their tracked podcasts with individual priorities and episode caps; the function runs on each playlist's configured schedule, fetching recent episodes and rebuilding the playlist using a priority-ordered algorithm that plays higher-priority shows first while keeping each show's episodes in chronological order.

## 2. User Stories

- **US-1**: As a user, I can define a list of tracked podcasts, each with a priority (1–10) and a per-podcast maximum number of episodes, so only the content I care about appears in my playlist.
- **US-2**: As a user, each playlist is automatically refreshed on its own configurable schedule (cron expression) without any manual intervention.
- **US-3**: As a user, I can specify the episode ordering for each podcast — oldest-first or newest-first — so serialized shows play in sequence while time-sensitive shows surface the latest episode first.
- **US-4**: As a user, episodes from higher-priority podcasts (lower number) always appear before lower-priority ones, so I listen to what matters most first.
- **US-5**: As a user, each podcast's episode count is capped individually so the playlist stays bounded without needing a global limit.
- **US-6**: As a user, episodes I have already finished listening to are automatically excluded from the playlist so I only see unplayed content.
- **US-7**: As a user, I can define multiple playlists, each with its own set of podcasts and configuration, so I can organize different listening contexts separately.
- **US-8**: As a user, I can set a maximum lookback period per podcast so that old, obsolete episodes are ignored.
- **US-9**: As a user, I can include or exclude episodes by regex match on episode title so I can filter out bonus content or target specific seasons.

## 3. Feature Requirements

### F1 — Podcast Configuration

A JSON configuration file defines the playlists to manage. Each playlist entry contains:

- The Spotify playlist ID to write to
- Update schedule as a cron expression (e.g., `0 0 * * *` for daily)
- A list of podcasts, each with:
  - Spotify show ID
  - Display name (for logging)
  - Priority (integer, 1–10; lower = more prominent)
  - Maximum episodes to include from that show
  - Episode ordering (`oldestFirst` or `newestFirst`, default: `oldestFirst`)
  - Maximum lookback in days (`maxLookbackDays`, optional)
  - Title include regex (`titleInclude`, optional) — only episodes with matching titles are kept
  - Title exclude regex (`titleExclude`, optional) — episodes with matching titles are removed

### F2 — Priority-based Episode Selection

For each configured podcast:

1. Fetch the most recent episodes from the Spotify API.
2. Exclude episodes the user has fully played.
3. If `maxLookbackDays` is set, exclude episodes published more than that many days ago.
4. If `titleInclude` is set, exclude episodes whose title does not match the regex.
5. If `titleExclude` is set, exclude episodes whose title matches the regex.
6. Order episodes according to the podcast's configured ordering (oldest-first or newest-first).
7. Cap the list at the podcast's configured `maxEpisodes`.

### F3 — Priority-Ordered Interleaving

Episodes are ordered by podcast priority, with all episodes from higher-priority podcasts appearing before lower-priority ones:

- Podcasts are grouped by priority level (1 = highest).
- Groups are processed in priority order (1 first, then 2, etc.).
- Within a group, episodes from same-priority podcasts are interleaved using round-robin, with each podcast's episodes in its configured order.

### F4 — Playlist Sync

Each run replaces the entire contents of the target Spotify playlist (declarative model). The playlist always reflects the latest configuration and episode state. There is no incremental diff or append logic.

### F5 — Finished Episode Exclusion

During episode selection, episodes the user has fully played (as reported by Spotify) are excluded before ordering and capping. This is always enabled and requires no configuration.

## 4. Configuration Model

### Playlist Configuration (`playlists.json`)

```json
{
  "playlists": [
    {
      "playlistId": "spotify-playlist-id",
      "schedule": "0 * * * *",
      "podcasts": [
        {
          "showId": "spotify-show-id",
          "name": "My Favorite Podcast",
          "priority": 2,
          "maxEpisodes": 10,
          "episodeOrder": "oldestFirst",
          "maxLookbackDays": 30,
          "titleExclude": "Bonus|Trailer"
        }
      ]
    }
  ]
}
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `Spotify__ClientId` | Spotify application client ID |
| `Spotify__ClientSecret` | Spotify application client secret |
| `Spotify__RefreshToken` | OAuth2 refresh token (obtained via one-time auth flow) |

## 5. Out of Scope (v1)

The following are explicitly out of scope for the initial version:

- **Web UI** — Configuration is file-based only.
- **Multiple playlist strategies** — Only priority-ordered interleaving is supported.
- **Notifications** — No alerts when the playlist is updated or errors occur.

