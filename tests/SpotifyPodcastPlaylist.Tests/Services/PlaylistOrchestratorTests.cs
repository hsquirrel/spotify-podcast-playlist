using NSubstitute;
using SpotifyPodcastPlaylist.Models;
using SpotifyPodcastPlaylist.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class PlaylistOrchestratorTests
{
    private readonly IConfigProvider _configProvider = Substitute.For<IConfigProvider>();
    private readonly IScheduleTracker _scheduleTracker = Substitute.For<IScheduleTracker>();
    private readonly IEpisodeSelector _episodeSelector = Substitute.For<IEpisodeSelector>();
    private readonly IPlaylistInterleaver _interleaver = Substitute.For<IPlaylistInterleaver>();
    private readonly ISpotifyClient _spotifyClient = Substitute.For<ISpotifyClient>();
    private readonly ILogger<PlaylistOrchestrator> _logger = Substitute.For<ILogger<PlaylistOrchestrator>>();

    // TODO: Phase 6 — PlaylistOrchestrator integration tests (tech spec section 9)
    //
    // These tests use mocked ISpotifyClient but real interleaver and selector
    // to verify end-to-end episode ordering and playlist replacement.
    //
    // Test: Full pipeline — episodes are selected, interleaved, and playlist is replaced
    // - Configure mock config with 2 podcasts at different priorities
    // - Configure mock schedule tracker to return isDue = true
    // - Configure mock Spotify client to return test episodes
    // - Use real PlaylistInterleaver
    // - Verify ReplacePlaylistTracksAsync is called with correctly ordered URIs
    //
    // Test: Playlist not due — skipped entirely
    // - Configure mock schedule tracker to return isDue = false
    // - Verify ReplacePlaylistTracksAsync is never called
    //
    // Test: Partial failure — one podcast fails, others still processed
    // - Configure one podcast's fetch to throw
    // - Verify playlist is still updated with episodes from successful podcasts
    //
    // Test: Playlist update failure — error logged, update time not recorded
    // - Configure ReplacePlaylistTracksAsync to throw
    // - Verify RecordUpdateAsync is NOT called for that playlist
    //
    // Test: Multiple playlists — each evaluated independently
    // - Configure two playlists, one due and one not
    // - Verify only the due playlist is processed

    [Fact]
    public async Task FullPipeline_SelectsInterleavesAndReplacesPlaylist()
    {
        // TODO: Implement with real PlaylistInterleaver + mocked Spotify client
    }

    [Fact]
    public async Task PlaylistNotDue_IsSkipped()
    {
        // TODO: Implement
    }

    [Fact]
    public async Task PartialPodcastFailure_StillUpdatesPlaylist()
    {
        // TODO: Implement
    }

    [Fact]
    public async Task PlaylistUpdateFailure_DoesNotRecordUpdate()
    {
        // TODO: Implement
    }

    [Fact]
    public async Task MultiplePlaylists_EachEvaluatedIndependently()
    {
        // TODO: Implement
    }
}
