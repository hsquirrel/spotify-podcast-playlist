using SpotifyPodcastPlaylist.Models;
using SpotifyPodcastPlaylist.Services;
using Xunit;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class PlaylistInterleaverTests
{
    private readonly PlaylistInterleaver _interleaver = new();

    [Fact]
    public void WorkedExample_ReturnsCorrectOrder()
    {
        var groups = new List<PodcastEpisodeGroup>
        {
            new() { Priority = 1, EpisodeUris = ["a1", "a2", "a3"] },
            new() { Priority = 1, EpisodeUris = ["b1", "b2"] },
            new() { Priority = 3, EpisodeUris = ["c1", "c2"] },
            new() { Priority = 3, EpisodeUris = ["d1"] },
        };

        var result = _interleaver.Interleave(groups);

        Assert.Equal(["a1", "b1", "a2", "b2", "a3", "c1", "d1", "c2"], result);
    }

    [Fact]
    public void EmptyGroups_ReturnsEmptyList()
    {
        var groups = new List<PodcastEpisodeGroup>
        {
            new() { Priority = 1, EpisodeUris = [] },
            new() { Priority = 2, EpisodeUris = [] },
        };

        var result = _interleaver.Interleave(groups);

        Assert.Empty(result);
    }

    [Fact]
    public void SinglePodcastInGroup_AppendsInOrder()
    {
        var groups = new List<PodcastEpisodeGroup>
        {
            new() { Priority = 1, EpisodeUris = ["x1", "x2", "x3"] },
        };

        var result = _interleaver.Interleave(groups);

        Assert.Equal(["x1", "x2", "x3"], result);
    }

    [Fact]
    public void PriorityOrdering_LowerNumberComesFirst()
    {
        var groups = new List<PodcastEpisodeGroup>
        {
            new() { Priority = 3, EpisodeUris = ["low1", "low2"] },
            new() { Priority = 1, EpisodeUris = ["high1", "high2"] },
        };

        var result = _interleaver.Interleave(groups);

        Assert.Equal(["high1", "high2", "low1", "low2"], result);
    }

    [Fact]
    public void RoundRobin_InterleavesEpisodesFromSamePriority()
    {
        var groups = new List<PodcastEpisodeGroup>
        {
            new() { Priority = 1, EpisodeUris = ["a1", "a2", "a3"] },
            new() { Priority = 1, EpisodeUris = ["b1", "b2"] },
        };

        var result = _interleaver.Interleave(groups);

        Assert.Equal(["a1", "b1", "a2", "b2", "a3"], result);
    }

    [Fact]
    public void EmptyQueueSkipped_DuringRoundRobin()
    {
        var groups = new List<PodcastEpisodeGroup>
        {
            new() { Priority = 1, EpisodeUris = ["a1", "a2"] },
            new() { Priority = 1, EpisodeUris = [] },
            new() { Priority = 1, EpisodeUris = ["c1"] },
        };

        var result = _interleaver.Interleave(groups);

        Assert.Equal(["a1", "c1", "a2"], result);
    }
}
