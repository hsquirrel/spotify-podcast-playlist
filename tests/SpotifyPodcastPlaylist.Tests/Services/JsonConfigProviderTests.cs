using NSubstitute;
using SpotifyPodcastPlaylist.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class JsonConfigProviderTests
{
    // TODO: Phase 6 — JsonConfigProvider tests (tech spec section 3 + section 9)
    //
    // Note: These tests will need to either:
    //   a. Write temp JSON files and point the provider at them, or
    //   b. Refactor JsonConfigProvider to accept a Stream/string for testability
    //
    // Test: Valid config parsed successfully
    // - Given a valid playlists.json with all required fields,
    //   verify it returns the correct PlaylistConfig objects
    //
    // Test: Missing playlistId is rejected
    // - Given JSON with empty/missing playlistId, verify it throws
    //
    // Test: Missing podcasts array is rejected
    // - Given JSON with no podcasts, verify it throws
    //
    // Test: Empty podcasts array is rejected
    // - Given JSON with podcasts: [], verify it throws
    //
    // Test: Priority range enforced (1-10)
    // - Given podcast with priority 0 or 11, verify it throws
    //
    // Test: MaxEpisodes must be >= 1
    // - Given podcast with maxEpisodes 0, verify it throws
    //
    // Test: Cron expression validated
    // - Given invalid cron expression, verify it throws
    // - Given valid 5-field cron, verify it parses
    //
    // Test: Invalid regex in titleInclude/titleExclude is rejected
    // - Given an invalid regex string like "[invalid", verify it throws
    //
    // Test: Optional fields default correctly
    // - Given podcast with no episodeOrder, verify it defaults to "oldestFirst"
    // - Given podcast with no maxLookbackDays, verify it's null

    [Fact]
    public void ValidConfig_ParsesSuccessfully()
    {
        // TODO: Implement
    }

    [Fact]
    public void MissingPlaylistId_Throws()
    {
        // TODO: Implement
    }

    [Fact]
    public void EmptyPodcasts_Throws()
    {
        // TODO: Implement
    }

    [Fact]
    public void PriorityOutOfRange_Throws()
    {
        // TODO: Implement
    }

    [Fact]
    public void InvalidCronExpression_Throws()
    {
        // TODO: Implement
    }

    [Fact]
    public void InvalidRegex_Throws()
    {
        // TODO: Implement
    }
}
