using NSubstitute;
using SpotifyPodcastPlaylist.Models;
using SpotifyPodcastPlaylist.Services;
using SpotifyAPI.Web;
using Microsoft.Extensions.Logging;
using Xunit;

using ISpotifyClient = SpotifyPodcastPlaylist.Services.ISpotifyClient;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class EpisodeSelectorTests
{
    private readonly ISpotifyClient _spotifyClient = Substitute.For<ISpotifyClient>();
    private readonly ILogger<EpisodeSelector> _logger = Substitute.For<ILogger<EpisodeSelector>>();

    private EpisodeSelector CreateSelector() => new(_spotifyClient, _logger);

    private static SimpleEpisode MakeEpisode(string uri, string name, string releaseDate, bool fullyPlayed = false)
    {
        return new SimpleEpisode
        {
            Uri = uri,
            Name = name,
            ReleaseDate = releaseDate,
            ResumePoint = new ResumePoint { FullyPlayed = fullyPlayed }
        };
    }

    private static PodcastConfig MakeConfig(
        int maxEpisodes = 100,
        string episodeOrder = "oldestFirst",
        int? maxLookbackDays = null,
        string? titleInclude = null,
        string? titleExclude = null)
    {
        return new PodcastConfig
        {
            ShowId = "show1",
            Name = "Test",
            Priority = 1,
            MaxEpisodes = maxEpisodes,
            EpisodeOrder = episodeOrder,
            MaxLookbackDays = maxLookbackDays,
            TitleInclude = titleInclude,
            TitleExclude = titleExclude,
        };
    }

    [Fact]
    public async Task OldestFirst_ReversesApiOrder()
    {
        // API returns newest-first
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("ep3", "Ep 3", "2025-01-03"),
            MakeEpisode("ep2", "Ep 2", "2025-01-02"),
            MakeEpisode("ep1", "Ep 1", "2025-01-01"),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig(episodeOrder: "oldestFirst"));

        Assert.Equal(new[] { "ep1", "ep2", "ep3" }, result.EpisodeUris);
    }

    [Fact]
    public async Task NewestFirst_PreservesApiOrder()
    {
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("ep3", "Ep 3", "2025-01-03"),
            MakeEpisode("ep2", "Ep 2", "2025-01-02"),
            MakeEpisode("ep1", "Ep 1", "2025-01-01"),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig(episodeOrder: "newestFirst"));

        Assert.Equal(new[] { "ep3", "ep2", "ep1" }, result.EpisodeUris);
    }

    [Fact]
    public async Task FinishedEpisodes_AreExcluded()
    {
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("ep3", "Ep 3", "2025-01-03"),
            MakeEpisode("ep2", "Ep 2", "2025-01-02", fullyPlayed: true),
            MakeEpisode("ep1", "Ep 1", "2025-01-01"),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig());

        Assert.Equal(new[] { "ep1", "ep3" }, result.EpisodeUris);
        Assert.DoesNotContain("ep2", result.EpisodeUris);
    }

    [Fact]
    public async Task LookbackFilter_ExcludesOldEpisodes()
    {
        var recent = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd");
        var old = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("recent", "Recent", recent),
            MakeEpisode("old", "Old", old),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig(maxLookbackDays: 7));

        Assert.Single(result.EpisodeUris);
        Assert.Equal("recent", result.EpisodeUris[0]);
    }

    [Fact]
    public async Task TitleInclude_KeepsOnlyMatching()
    {
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("ep3", "Season 2 - Episode 3", "2025-01-03"),
            MakeEpisode("ep2", "Season 1 - Episode 2", "2025-01-02"),
            MakeEpisode("ep1", "Season 2 - Episode 1", "2025-01-01"),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig(titleInclude: "Season 2"));

        Assert.Equal(new[] { "ep1", "ep3" }, result.EpisodeUris);
    }

    [Fact]
    public async Task TitleExclude_RemovesMatching()
    {
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("ep3", "Episode 3", "2025-01-03"),
            MakeEpisode("ep2", "Bonus: Extra", "2025-01-02"),
            MakeEpisode("ep1", "Trailer", "2025-01-01"),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig(titleExclude: "Bonus|Trailer"));

        Assert.Single(result.EpisodeUris);
        Assert.Equal("ep3", result.EpisodeUris[0]);
    }

    [Fact]
    public async Task MaxEpisodes_CapsResult()
    {
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>
        {
            MakeEpisode("ep5", "Ep 5", "2025-01-05"),
            MakeEpisode("ep4", "Ep 4", "2025-01-04"),
            MakeEpisode("ep3", "Ep 3", "2025-01-03"),
            MakeEpisode("ep2", "Ep 2", "2025-01-02"),
            MakeEpisode("ep1", "Ep 1", "2025-01-01"),
        });

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig(maxEpisodes: 3));

        Assert.Equal(3, result.EpisodeUris.Count);
        // oldestFirst default, so first 3 oldest
        Assert.Equal(new[] { "ep1", "ep2", "ep3" }, result.EpisodeUris);
    }

    [Fact]
    public async Task EmptyShow_ReturnsEmptyGroup()
    {
        _spotifyClient.GetShowEpisodesAsync("show1").Returns(new List<SimpleEpisode>());

        var result = await CreateSelector().SelectEpisodesAsync(MakeConfig());

        Assert.Empty(result.EpisodeUris);
        Assert.Equal(1, result.Priority);
    }
}
