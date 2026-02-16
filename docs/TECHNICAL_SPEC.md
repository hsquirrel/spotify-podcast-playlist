# Technical Specification — Spotify Podcast Playlist

## 1. Architecture Overview

The system runs as an Azure Function on a timer trigger (every minute). Each execution evaluates per-playlist cron schedules and only processes playlists that are due for an update.

```
Timer Trigger (every minute)
  → Load playlist configuration from JSON
  → Authenticate with Spotify (OAuth2 token refresh)
  → For each configured playlist:
      → Evaluate playlist's cron schedule against last update time
      → If not due, skip
      → Fetch episodes for each podcast
      → Select and filter episodes (exclude finished, cap per podcast)
      → Interleave episodes using priority-ordered grouping
      → Replace playlist contents via Spotify API
      → Record last update time
```

## 2. Project Structure

```
src/SpotifyPodcastPlaylist/
  Functions/
    UpdatePlaylistFunction.cs        # Timer-triggered Azure Function
  Services/
    ISpotifyClient.cs                # Abstraction over Spotify API
    SpotifyClientWrapper.cs          # Implementation using SpotifyAPI.Web
    IConfigProvider.cs               # Configuration loading interface
    JsonConfigProvider.cs            # Loads playlists.json
    IPlaylistInterleaver.cs          # Interleaving algorithm interface
    PlaylistInterleaver.cs           # Priority-ordered interleaving implementation
    IEpisodeSelector.cs              # Episode fetch + selection interface
    EpisodeSelector.cs               # Fetches and caps episodes per podcast
    PlaylistOrchestrator.cs          # Coordinates the full pipeline
    IScheduleTracker.cs              # Schedule evaluation + last update tracking interface
    BlobScheduleTracker.cs           # Implementation using Azure Blob Storage
  Models/
    PlaylistConfig.cs                # Playlist-level config model
    PodcastConfig.cs                 # Per-podcast config model
    PodcastEpisodeGroup.cs           # A podcast's selected episodes
  Config/
    playlists.json                   # User configuration file
tests/SpotifyPodcastPlaylist.Tests/
  Services/
    PlaylistInterleaverTests.cs
    EpisodeSelectorTests.cs
    JsonConfigProviderTests.cs
    PlaylistOrchestratorTests.cs
```

## 3. Configuration Schema

### `playlists.json`

```json
{
  "playlists": [
    {
      "playlistId": "string — Spotify playlist ID",
      "schedule": "0 * * * *",
      "podcasts": [
        {
          "showId": "string — Spotify show ID",
          "name": "string — display name for logging",
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

**Validation rules:**
- `playlistId` — required, non-empty string
- `schedule` — required, valid cron expression (5-field format: minute, hour, day-of-month, month, day-of-week)
- `podcasts` — required, at least one entry
- `showId` — required, non-empty string
- `priority` — required, integer 1–10
- `maxEpisodes` — required, integer ≥ 1
- `episodeOrder` — optional, one of `"oldestFirst"` or `"newestFirst"`, defaults to `"oldestFirst"`
- `maxLookbackDays` — optional, integer ≥ 1
- `titleInclude` — optional, valid regex string
- `titleExclude` — optional, valid regex string

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `Spotify__ClientId` | Yes | Spotify app client ID |
| `Spotify__ClientSecret` | Yes | Spotify app client secret |
| `Spotify__RefreshToken` | Yes | OAuth2 refresh token |
| `AzureWebJobsStorage` | Yes | Azure Storage connection string (used for schedule tracking) |

These are read via the standard Azure Functions configuration system (`IConfiguration`).

## 4. Schedule Evaluation

The Azure Function's timer trigger fires every minute. On each tick, the function evaluates whether each playlist is due for an update:

1. Load the playlist's last update timestamp from Azure Blob Storage.
2. Parse the playlist's cron expression and determine the most recent scheduled time.
3. If the most recent scheduled time is after the last update timestamp, the playlist is due — process it.
4. After successfully updating a playlist, record the current time as its last update timestamp.
5. On first run (no stored timestamp), the playlist is always processed.

Last update timestamps are stored as a JSON file in Azure Blob Storage, keyed by playlist ID. The `AzureWebJobsStorage` connection string (standard for Azure Functions) is used for storage.

## 5. Interleaving Algorithm

The algorithm has two phases: **selection** and **interleaving**.

### Phase 1 — Selection

For each podcast in the configuration:

1. Fetch episodes from `GET /shows/{id}/episodes` (most recent first from API).
2. Exclude episodes where `resume_point.fully_played` is `true`.
3. If `maxLookbackDays` is configured, exclude episodes where `release_date` is more than that many days before the current time.
4. If `titleInclude` is configured, exclude episodes whose `name` does not match the regex.
5. If `titleExclude` is configured, exclude episodes whose `name` matches the regex.
6. Order according to the podcast's configured `episodeOrder` (oldest-first or newest-first). Default: oldest-first.
7. Cap the list at `maxEpisodes`.

The result is a `PodcastEpisodeGroup` per podcast: a queue of episode URIs in the configured order, tagged with the podcast's priority.

### Phase 2 — Priority-Ordered Interleaving

1. Group podcasts by priority level.
2. Process groups in ascending priority order (1 first).
3. Within each group, interleave episodes round-robin: take one episode from each podcast in turn (in the order podcasts appear in configuration), repeating until all queues in the group are exhausted. Each podcast's episodes are dequeued in its configured order.
4. Append the interleaved group to the final playlist.
5. Repeat for the next priority group.

### Worked Example

Four podcasts configured:

| Podcast | Priority | Episodes |
|---------|----------|----------|
| A | 1 | a1, a2, a3 |
| B | 1 | b1, b2 |
| C | 3 | c1, c2 |
| D | 3 | d1 |

**Group 1 (priority 1):** Podcasts A and B

Round-robin interleave: a1, b1, a2, b2, a3

**Group 2 (priority 3):** Podcasts C and D

Round-robin interleave: c1, d1, c2

**Final playlist**: `[a1, b1, a2, b2, a3, c1, d1, c2]`

### Edge Cases

- **Empty queue**: If a podcast has no episodes, it is skipped during round-robin.
- **Single podcast in a group**: Its episodes are appended in order with no interleaving.
- **All episodes finished**: If all fetched episodes are fully played, the podcast's queue is empty and it is skipped.
- **Empty priority group**: If no podcasts exist at a priority level, that level is skipped.
- **All episodes filtered**: If all fetched episodes are excluded by lookback/regex filters, the podcast's queue is empty and it is skipped.

## 6. Spotify API Integration

### Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/shows/{id}/episodes` | GET | Fetch recent episodes for a podcast |
| `/playlists/{id}/tracks` | GET | Read current playlist contents (for logging) |
| `/playlists/{id}/tracks` | PUT | Replace playlist contents |

### Library

Using the [`SpotifyAPI.Web`](https://github.com/JohnnyCrazy/SpotifyAPI-NET) NuGet package (v7.x). This library provides:

- Typed API wrappers for all endpoints
- Automatic pagination
- Built-in rate limit handling (retry on 429)

### Rate Limiting

The `SpotifyAPI.Web` library automatically retries on HTTP 429 responses with the `Retry-After` header. No custom rate limiting logic is needed.

### Playlist Size Limits

Spotify playlists support up to 10,000 items. The `PUT /playlists/{id}/tracks` endpoint accepts up to 100 URIs per request. For playlists larger than 100 items, multiple requests are needed.

## 7. Authentication

### Initial Setup (One-Time)

OAuth2 Authorization Code flow:

1. User registers a Spotify application at [developer.spotify.com](https://developer.spotify.com).
2. A helper tool/script opens the browser for authorization, requesting scopes: `playlist-modify-public`, `playlist-modify-private`, `playlist-read-private`, `user-read-playback-position`.
3. The callback captures the authorization code and exchanges it for access + refresh tokens.
4. The refresh token is stored in the `Spotify__RefreshToken` environment variable.

### Runtime

At each function execution:

1. Create an `AuthorizationCodeAuthenticator` with the client ID, client secret, and refresh token.
2. The `SpotifyAPI.Web` library automatically refreshes the access token when it expires.
3. Construct a `SpotifyClient` with the authenticator.

## 8. Error Handling

| Scenario | Behavior |
|----------|----------|
| 401 Unauthorized | Refresh token may be invalid; log error, skip run |
| 429 Rate Limited | Handled automatically by `SpotifyAPI.Web` retry logic |
| 404 Show Not Found | Log warning, skip that podcast, continue with others |
| 404 Playlist Not Found | Log error, skip that playlist |
| Invalid configuration | Fail loudly at startup with descriptive error message |
| Partial fetch failure | Build playlist from successfully fetched podcasts |
| Network timeout | Let Azure Functions retry on next timer tick |

The general philosophy is: fail loudly for configuration errors (programmer mistakes), fail gracefully for runtime/API errors (transient issues).

## 9. Testing Approach

### Framework

- **xUnit** for test framework
- **Moq** or **NSubstitute** for mocking interfaces
- Tests live in `tests/SpotifyPodcastPlaylist.Tests/`

### Unit Tests

| Component | Key Test Cases |
|-----------|---------------|
| `PlaylistInterleaver` | Priority group ordering, round-robin within group, empty queues skipped, single podcast in group, worked example from spec |
| `EpisodeSelector` | Oldest-first ordering, newest-first ordering, default ordering, finished episodes excluded, lookback filtering, title include regex, title exclude regex, combined filters, maxEpisodes cap, empty show handling |
| `JsonConfigProvider` | Valid config parsed, missing fields rejected, priority range enforced, cron expression validated |
| `ScheduleTracker` | Playlist due for update, playlist not due, first run with no prior timestamp |

### Integration Tests

| Component | Approach |
|-----------|----------|
| `PlaylistOrchestrator` | Mock `ISpotifyClient`, provide real interleaver and selector, verify end-to-end episode ordering and playlist replacement call |

## 10. Implementation Phases

| Phase | Scope | Key Deliverables |
|-------|-------|-----------------|
| 1 | Config + Models + DI wiring | `PlaylistConfig`, `PodcastConfig`, `PodcastEpisodeGroup`, `JsonConfigProvider`, `IScheduleTracker`, `BlobScheduleTracker`, DI registration in `Program.cs` |
| 2 | Spotify API integration | `ISpotifyClient`, `SpotifyClientWrapper`, auth setup, episode fetching, playlist replacement |
| 3 | Episode selection logic | `IEpisodeSelector`, `EpisodeSelector` — fetch, reorder, cap |
| 4 | Interleaving algorithm | `IPlaylistInterleaver`, `PlaylistInterleaver` — priority-ordered interleaving |
| 5 | Orchestration + function wiring | `PlaylistOrchestrator`, `UpdatePlaylistFunction` updated to use orchestrator |
| 6 | Unit and integration tests | Full test suite for all components |
| 7 | Auth helper | CLI tool or script for initial OAuth2 token acquisition |
