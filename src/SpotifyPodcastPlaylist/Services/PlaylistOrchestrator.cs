using Microsoft.Extensions.Logging;

namespace SpotifyPodcastPlaylist.Services;

public class PlaylistOrchestrator
{
    private readonly IConfigProvider _configProvider;
    private readonly IScheduleTracker _scheduleTracker;
    private readonly IEpisodeSelector _episodeSelector;
    private readonly IPlaylistInterleaver _interleaver;
    private readonly ISpotifyClient _spotifyClient;
    private readonly ILogger<PlaylistOrchestrator> _logger;

    public PlaylistOrchestrator(
        IConfigProvider configProvider,
        IScheduleTracker scheduleTracker,
        IEpisodeSelector episodeSelector,
        IPlaylistInterleaver interleaver,
        ISpotifyClient spotifyClient,
        ILogger<PlaylistOrchestrator> logger)
    {
        _configProvider = configProvider;
        _scheduleTracker = scheduleTracker;
        _episodeSelector = episodeSelector;
        _interleaver = interleaver;
        _spotifyClient = spotifyClient;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // TODO: Phase 5 — Orchestration (tech spec sections 1, 4, 5, 6, 8)
        //
        // 1. Load configuration:
        //    - var playlists = _configProvider.GetPlaylists()
        //    - If config fails, it throws (fail loudly — tech spec section 8)
        //
        // 2. For each playlist in playlists:
        //    a. Check schedule:
        //       - var isDue = await _scheduleTracker.IsDueAsync(playlist.PlaylistId, playlist.Schedule)
        //       - If not due, log and skip to next playlist
        //
        //    b. Select episodes for each podcast:
        //       - var episodeGroups = new List<PodcastEpisodeGroup>()
        //       - For each podcast in playlist.Podcasts:
        //         - try:
        //           - var group = await _episodeSelector.SelectEpisodesAsync(podcast)
        //           - episodeGroups.Add(group)
        //         - catch (exception):
        //           - Log warning: "Failed to fetch episodes for {podcast.Name}: {error}"
        //           - Continue with other podcasts (partial failure — tech spec section 8)
        //
        //    c. Interleave episodes:
        //       - var orderedUris = _interleaver.Interleave(episodeGroups)
        //       - Log: "Interleaved {count} episodes for playlist {playlist.PlaylistId}"
        //
        //    d. Replace playlist contents:
        //       - try:
        //         - await _spotifyClient.ReplacePlaylistTracksAsync(playlist.PlaylistId, orderedUris)
        //         - Log: "Successfully updated playlist {playlist.PlaylistId}"
        //       - catch (exception):
        //         - Log error: "Failed to update playlist {playlist.PlaylistId}: {error}"
        //         - Continue to next playlist (don't record update time)
        //         - continue
        //
        //    e. Record update time:
        //       - await _scheduleTracker.RecordUpdateAsync(playlist.PlaylistId)
    }
}
