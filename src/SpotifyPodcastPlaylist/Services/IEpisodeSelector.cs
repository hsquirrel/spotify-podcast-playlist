using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

/// <summary>
/// Fetches and selects episodes for a single podcast based on its configuration.
/// </summary>
public interface IEpisodeSelector
{
    /// <summary>
    /// Fetches episodes for a podcast and applies the 7-step selection pipeline:
    /// 1. Fetch episodes from Spotify API
    /// 2. Exclude fully-played episodes
    /// 3. Apply maxLookbackDays filter
    /// 4. Apply titleInclude regex filter
    /// 5. Apply titleExclude regex filter
    /// 6. Order by episodeOrder (oldestFirst or newestFirst)
    /// 7. Cap at maxEpisodes
    /// Returns a PodcastEpisodeGroup with the podcast's priority and selected episode URIs.
    /// </summary>
    Task<PodcastEpisodeGroup> SelectEpisodesAsync(PodcastConfig config);
}
