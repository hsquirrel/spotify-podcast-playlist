using Microsoft.Extensions.Logging;
using SpotifyPodcastPlaylist.Models;

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
        // Config loading throws on failure (fail loudly)
        var playlists = _configProvider.GetPlaylists();
        _logger.LogInformation("Processing {Count} configured playlist(s)", playlists.Count);

        foreach (var playlist in playlists)
        {
            var isDue = await _scheduleTracker.IsDueAsync(playlist.PlaylistId, playlist.Schedule);
            if (!isDue)
            {
                _logger.LogInformation("Playlist {PlaylistId} is not due, skipping", playlist.PlaylistId);
                continue;
            }

            // Select episodes for each podcast (partial failure tolerance)
            var episodeGroups = new List<PodcastEpisodeGroup>();
            foreach (var podcast in playlist.Podcasts)
            {
                try
                {
                    var group = await _episodeSelector.SelectEpisodesAsync(podcast);
                    episodeGroups.Add(group);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch episodes for {PodcastName}, skipping", podcast.Name);
                }
            }

            // Interleave episodes
            var orderedUris = _interleaver.Interleave(episodeGroups);
            _logger.LogInformation("Interleaved {Count} episodes for playlist {PlaylistId}",
                orderedUris.Count, playlist.PlaylistId);

            // Replace playlist contents
            try
            {
                await _spotifyClient.ReplacePlaylistTracksAsync(playlist.PlaylistId, orderedUris);
                _logger.LogInformation("Successfully updated playlist {PlaylistId}", playlist.PlaylistId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update playlist {PlaylistId}", playlist.PlaylistId);
                continue; // Don't record update time if replace failed
            }

            // Record update time
            await _scheduleTracker.RecordUpdateAsync(playlist.PlaylistId);
        }
    }
}
