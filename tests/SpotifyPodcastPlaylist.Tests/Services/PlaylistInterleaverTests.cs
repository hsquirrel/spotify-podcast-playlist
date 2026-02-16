using SpotifyPodcastPlaylist.Models;
using SpotifyPodcastPlaylist.Services;
using Xunit;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class PlaylistInterleaverTests
{
    private readonly PlaylistInterleaver _interleaver = new();

    // TODO: Phase 6 — PlaylistInterleaver tests (tech spec section 5 + section 9)
    //
    // Test: Priority group ordering
    // - Given groups at priorities 3 and 1, verify priority-1 episodes come first
    //
    // Test: Round-robin interleaving within a priority group
    // - Given two podcasts at same priority with [a1,a2,a3] and [b1,b2],
    //   verify result is [a1, b1, a2, b2, a3]
    //
    // Test: Empty queues are skipped during round-robin
    // - Given one podcast with episodes and one with empty list at same priority,
    //   verify the empty one is skipped and non-empty episodes appear in order
    //
    // Test: Single podcast in a priority group
    // - Given one podcast at priority 1, verify its episodes appear in order
    //
    // Test: Worked example from spec
    // - A(pri=1): [a1,a2,a3], B(pri=1): [b1,b2], C(pri=3): [c1,c2], D(pri=3): [d1]
    //   Expected: [a1, b1, a2, b2, a3, c1, d1, c2]
    //
    // Test: All groups empty
    // - Given groups with no episodes, verify empty list returned
    //
    // Test: Multiple priority levels (more than 2)
    // - Verify groups at priorities 1, 2, 5 are processed in order

    [Fact]
    public void WorkedExample_ReturnsCorrectOrder()
    {
        // TODO: Implement — this is the worked example from tech spec section 5
        // var groups = new List<PodcastEpisodeGroup>
        // {
        //     new() { Priority = 1, EpisodeUris = ["a1", "a2", "a3"] },
        //     new() { Priority = 1, EpisodeUris = ["b1", "b2"] },
        //     new() { Priority = 3, EpisodeUris = ["c1", "c2"] },
        //     new() { Priority = 3, EpisodeUris = ["d1"] },
        // };
        // var result = _interleaver.Interleave(groups);
        // Assert.Equal(["a1", "b1", "a2", "b2", "a3", "c1", "d1", "c2"], result);
    }

    [Fact]
    public void EmptyGroups_ReturnsEmptyList()
    {
        // TODO: Implement
    }

    [Fact]
    public void SinglePodcastInGroup_AppendsInOrder()
    {
        // TODO: Implement
    }

    [Fact]
    public void PriorityOrdering_LowerNumberComesFirst()
    {
        // TODO: Implement
    }

    [Fact]
    public void RoundRobin_InterleavesEpisodesFromSamePriority()
    {
        // TODO: Implement
    }

    [Fact]
    public void EmptyQueueSkipped_DuringRoundRobin()
    {
        // TODO: Implement
    }
}
