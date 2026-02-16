using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SpotifyPodcastPlaylist.Models;

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
        // TODO: Phase 3 — Episode selection logic (tech spec section 5, Phase 1)
        //
        // Implement the 7-step selection pipeline:
        //
        // Step 1: Fetch episodes from Spotify API
        //   - var episodes = await _spotifyClient.GetShowEpisodesAsync(config.ShowId)
        //   - API returns newest-first by default
        //   - Log: "Fetched {count} episodes for {config.Name}"
        //
        // Step 2: Exclude fully-played episodes
        //   - episodes = episodes.Where(e => e.ResumePoint?.FullyPlayed != true)
        //   - Log count of excluded episodes
        //
        // Step 3: Apply maxLookbackDays filter
        //   - If config.MaxLookbackDays has a value:
        //     - var cutoff = DateTime.UtcNow.AddDays(-config.MaxLookbackDays.Value)
        //     - Parse each episode's ReleaseDate (format: "yyyy-MM-dd") and exclude if before cutoff
        //     - Log count of excluded episodes
        //
        // Step 4: Apply titleInclude regex filter
        //   - If config.TitleInclude is not null/empty:
        //     - var regex = new Regex(config.TitleInclude, RegexOptions.IgnoreCase)
        //     - Keep only episodes where regex.IsMatch(episode.Name)
        //     - Log count of excluded episodes
        //
        // Step 5: Apply titleExclude regex filter
        //   - If config.TitleExclude is not null/empty:
        //     - var regex = new Regex(config.TitleExclude, RegexOptions.IgnoreCase)
        //     - Remove episodes where regex.IsMatch(episode.Name)
        //     - Log count of excluded episodes
        //
        // Step 6: Order by episodeOrder
        //   - If config.EpisodeOrder == "newestFirst":
        //     - Episodes are already newest-first from API, so no re-ordering needed
        //   - If config.EpisodeOrder == "oldestFirst" (default):
        //     - Reverse the list (or sort by ReleaseDate ascending)
        //
        // Step 7: Cap at maxEpisodes
        //   - episodes = episodes.Take(config.MaxEpisodes)
        //
        // Return:
        //   new PodcastEpisodeGroup
        //   {
        //       Priority = config.Priority,
        //       EpisodeUris = episodes.Select(e => e.Uri).ToList()
        //   }

        throw new NotImplementedException();
    }
}
