using NSubstitute;
using SpotifyPodcastPlaylist.Models;
using SpotifyPodcastPlaylist.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class EpisodeSelectorTests
{
    private readonly ISpotifyClient _spotifyClient = Substitute.For<ISpotifyClient>();
    private readonly ILogger<EpisodeSelector> _logger = Substitute.For<ILogger<EpisodeSelector>>();

    // TODO: Phase 6 — EpisodeSelector tests (tech spec section 5 Phase 1 + section 9)
    //
    // Test: Oldest-first ordering
    // - Given episodes returned newest-first from API and config.EpisodeOrder = "oldestFirst",
    //   verify episodes are reversed (oldest first in result)
    //
    // Test: Newest-first ordering
    // - Given episodes from API and config.EpisodeOrder = "newestFirst",
    //   verify episodes stay in API order (newest first)
    //
    // Test: Default ordering is oldest-first
    // - Given config with no explicit episodeOrder, verify oldest-first behavior
    //
    // Test: Finished episodes excluded
    // - Given episodes where some have ResumePoint.FullyPlayed = true,
    //   verify those are excluded from the result
    //
    // Test: Lookback filtering
    // - Given config.MaxLookbackDays = 7 and episodes older than 7 days,
    //   verify old episodes are excluded
    //
    // Test: Title include regex
    // - Given config.TitleInclude = "Season 2" and mixed episode titles,
    //   verify only matching episodes are kept
    //
    // Test: Title exclude regex
    // - Given config.TitleExclude = "Bonus|Trailer" and mixed titles,
    //   verify matching episodes are removed
    //
    // Test: Combined filters
    // - Given multiple filters active at once, verify they all apply in sequence
    //
    // Test: MaxEpisodes cap
    // - Given 10 episodes and config.MaxEpisodes = 3, verify only 3 returned
    //
    // Test: Empty show (no episodes)
    // - Given a show with no episodes, verify empty EpisodeUris list returned

    [Fact]
    public void OldestFirst_ReversesApiOrder()
    {
        // TODO: Implement
    }

    [Fact]
    public void NewestFirst_PreservesApiOrder()
    {
        // TODO: Implement
    }

    [Fact]
    public void FinishedEpisodes_AreExcluded()
    {
        // TODO: Implement
    }

    [Fact]
    public void LookbackFilter_ExcludesOldEpisodes()
    {
        // TODO: Implement
    }

    [Fact]
    public void TitleInclude_KeepsOnlyMatching()
    {
        // TODO: Implement
    }

    [Fact]
    public void TitleExclude_RemovesMatching()
    {
        // TODO: Implement
    }

    [Fact]
    public void MaxEpisodes_CapsResult()
    {
        // TODO: Implement
    }

    [Fact]
    public void EmptyShow_ReturnsEmptyGroup()
    {
        // TODO: Implement
    }
}
