using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Cronos;
using Microsoft.Extensions.Logging;
using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

public class JsonConfigProvider : IConfigProvider
{
    private readonly ILogger<JsonConfigProvider> _logger;
    private readonly string _configPath;

    public JsonConfigProvider(ILogger<JsonConfigProvider> logger)
        : this(logger, Path.Combine(AppContext.BaseDirectory, "Config", "playlists.json"))
    {
    }

    internal JsonConfigProvider(ILogger<JsonConfigProvider> logger, string configPath)
    {
        _logger = logger;
        _configPath = configPath;
    }

    public List<PlaylistConfig> GetPlaylists()
    {
        if (!File.Exists(_configPath))
            throw new FileNotFoundException($"Configuration file not found: {_configPath}");

        var json = File.ReadAllText(_configPath);

        var wrapper = JsonSerializer.Deserialize<ConfigWrapper>(json);
        if (wrapper?.Playlists is null || wrapper.Playlists.Count == 0)
            throw new InvalidOperationException("Configuration must contain at least one playlist.");

        foreach (var playlist in wrapper.Playlists)
        {
            ValidatePlaylist(playlist);
        }

        _logger.LogInformation("Loaded {PlaylistCount} playlist(s) from configuration", wrapper.Playlists.Count);
        return wrapper.Playlists;
    }

    private static void ValidatePlaylist(PlaylistConfig playlist)
    {
        if (string.IsNullOrWhiteSpace(playlist.PlaylistId))
            throw new InvalidOperationException("Playlist playlistId is required and cannot be empty.");

        try
        {
            CronExpression.Parse(playlist.Schedule);
        }
        catch (CronFormatException ex)
        {
            throw new InvalidOperationException($"Invalid cron expression '{playlist.Schedule}': {ex.Message}", ex);
        }

        if (playlist.Podcasts is null || playlist.Podcasts.Count == 0)
            throw new InvalidOperationException($"Playlist '{playlist.PlaylistId}' must have at least one podcast.");

        foreach (var podcast in playlist.Podcasts)
        {
            ValidatePodcast(playlist.PlaylistId, podcast);
        }
    }

    private static void ValidatePodcast(string playlistId, PodcastConfig podcast)
    {
        if (string.IsNullOrWhiteSpace(podcast.ShowId))
            throw new InvalidOperationException($"Podcast showId is required in playlist '{playlistId}'.");

        if (string.IsNullOrWhiteSpace(podcast.Name))
            throw new InvalidOperationException($"Podcast name is required in playlist '{playlistId}'.");

        if (podcast.Priority < 1 || podcast.Priority > 10)
            throw new InvalidOperationException($"Podcast '{podcast.Name}' priority must be between 1 and 10, got {podcast.Priority}.");

        if (podcast.MaxEpisodes < 1)
            throw new InvalidOperationException($"Podcast '{podcast.Name}' maxEpisodes must be at least 1, got {podcast.MaxEpisodes}.");

        if (podcast.EpisodeOrder is not ("oldestFirst" or "newestFirst"))
            throw new InvalidOperationException($"Podcast '{podcast.Name}' episodeOrder must be 'oldestFirst' or 'newestFirst', got '{podcast.EpisodeOrder}'.");

        if (podcast.MaxLookbackDays.HasValue && podcast.MaxLookbackDays.Value < 1)
            throw new InvalidOperationException($"Podcast '{podcast.Name}' maxLookbackDays must be at least 1, got {podcast.MaxLookbackDays}.");

        ValidateRegex(podcast.TitleInclude, podcast.Name, "titleInclude");
        ValidateRegex(podcast.TitleExclude, podcast.Name, "titleExclude");
    }

    private static void ValidateRegex(string? pattern, string podcastName, string fieldName)
    {
        if (string.IsNullOrEmpty(pattern))
            return;

        try
        {
            _ = new Regex(pattern);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Podcast '{podcastName}' has invalid regex for {fieldName}: '{pattern}'. {ex.Message}", ex);
        }
    }

    private class ConfigWrapper
    {
        [JsonPropertyName("playlists")]
        public List<PlaylistConfig> Playlists { get; set; } = new();
    }
}
