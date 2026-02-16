using System.Text.Json;
using Microsoft.Extensions.Logging;
using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

public class JsonConfigProvider : IConfigProvider
{
    private readonly ILogger<JsonConfigProvider> _logger;

    public JsonConfigProvider(ILogger<JsonConfigProvider> logger)
    {
        _logger = logger;
    }

    public List<PlaylistConfig> GetPlaylists()
    {
        // TODO: Phase 1 — Configuration loading (tech spec section 3)
        //
        // 1. Determine the path to playlists.json:
        //    - Resolve relative to the application's base directory
        //    - Path: Config/playlists.json
        //
        // 2. Read the file contents:
        //    - File.ReadAllText(path)
        //    - If file not found, throw with descriptive error
        //
        // 3. Deserialize the JSON:
        //    - Define a wrapper class: { "playlists": List<PlaylistConfig> }
        //    - Use JsonSerializer.Deserialize with case-insensitive options
        //    - If deserialization returns null, throw
        //
        // 4. Validate each playlist (fail loudly per tech spec section 8):
        //    a. playlistId — required, non-empty string
        //    b. schedule — required, valid 5-field cron expression
        //       - Use Cronos.CronExpression.Parse() to validate; catch CronFormatException
        //    c. podcasts — required, at least one entry
        //
        // 5. Validate each podcast within each playlist:
        //    a. showId — required, non-empty string
        //    b. name — required, non-empty string
        //    c. priority — required, integer 1–10
        //    d. maxEpisodes — required, integer >= 1
        //    e. episodeOrder — if provided, must be "oldestFirst" or "newestFirst"
        //    f. maxLookbackDays — if provided, must be >= 1
        //    g. titleInclude — if provided, must be valid regex (try Regex constructor)
        //    h. titleExclude — if provided, must be valid regex (try Regex constructor)
        //
        // 6. Log the number of playlists and podcasts loaded
        //
        // 7. Return the validated list

        throw new NotImplementedException();
    }
}
