using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SpotifyPodcastPlaylist.Models;
using SpotifyAPI.Web;

namespace SpotifyPodcastPlaylist.Services;

public class EpisodeSelector : IEpisodeSelector
{
    private readonly ISpotifyClient _spotifyClient;
    private readonly ILogger<EpisodeSelector> _logger;

    public EpisodeSelector(ISpotifyClient spotifyClient, ILogger<EpisodeSelector> logger)
    {
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    public async Task<PodcastEpisodeGroup> SelectEpisodesAsync(PodcastConfig config)
    {
        // Step 1: Fetch episodes from Spotify API (newest-first from API)
        var episodes = await _spotifyClient.GetShowEpisodesAsync(config.ShowId);
        _logger.LogInformation("Fetched {Count} episodes for {Name}", episodes.Count, config.Name);

        // Step 2: Exclude fully-played episodes
        var filtered = episodes.Where(e => e.ResumePoint?.FullyPlayed != true).ToList();
        var excludedPlayed = episodes.Count - filtered.Count;
        if (excludedPlayed > 0)
            _logger.LogInformation("Excluded {Count} fully-played episodes for {Name}", excludedPlayed, config.Name);

        // Step 3: Apply maxLookbackDays filter
        if (config.MaxLookbackDays.HasValue)
        {
            var cutoff = DateTime.UtcNow.AddDays(-config.MaxLookbackDays.Value);
            var before = filtered.Count;
            filtered = filtered.Where(e => ParseReleaseDate(e.ReleaseDate) >= cutoff).ToList();
            var excludedLookback = before - filtered.Count;
            if (excludedLookback > 0)
                _logger.LogInformation("Excluded {Count} episodes older than {Days} days for {Name}", excludedLookback, config.MaxLookbackDays.Value, config.Name);
        }

        // Step 4: Apply titleInclude regex filter
        if (!string.IsNullOrEmpty(config.TitleInclude))
        {
            var regex = new Regex(config.TitleInclude, RegexOptions.IgnoreCase);
            var before = filtered.Count;
            filtered = filtered.Where(e => regex.IsMatch(e.Name)).ToList();
            var excluded = before - filtered.Count;
            if (excluded > 0)
                _logger.LogInformation("Excluded {Count} episodes not matching titleInclude for {Name}", excluded, config.Name);
        }

        // Step 5: Apply titleExclude regex filter
        if (!string.IsNullOrEmpty(config.TitleExclude))
        {
            var regex = new Regex(config.TitleExclude, RegexOptions.IgnoreCase);
            var before = filtered.Count;
            filtered = filtered.Where(e => !regex.IsMatch(e.Name)).ToList();
            var excluded = before - filtered.Count;
            if (excluded > 0)
                _logger.LogInformation("Excluded {Count} episodes matching titleExclude for {Name}", excluded, config.Name);
        }

        // Step 6: Order by episodeOrder (API returns newest-first)
        if (config.EpisodeOrder != "newestFirst")
        {
            filtered.Reverse();
        }

        // Step 7: Cap at maxEpisodes
        filtered = filtered.Take(config.MaxEpisodes).ToList();

        return new PodcastEpisodeGroup
        {
            Priority = config.Priority,
            EpisodeUris = filtered.Select(e => e.Uri).ToList()
        };
    }

    private static DateTime ParseReleaseDate(string releaseDate)
    {
        // Spotify returns dates in various formats: yyyy-MM-dd, yyyy-MM, yyyy
        if (DateTime.TryParseExact(releaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            return date;
        if (DateTime.TryParseExact(releaseDate, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            return date;
        if (DateTime.TryParseExact(releaseDate, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
            return date;
        return DateTime.MinValue;
    }
}
