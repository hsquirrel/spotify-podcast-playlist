using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SpotifyPodcastPlaylist.Models;
using SpotifyPodcastPlaylist.Services;
using Microsoft.Extensions.Logging;
using Xunit;

using ISpotifyClient = SpotifyPodcastPlaylist.Services.ISpotifyClient;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class PlaylistOrchestratorTests
{
    private readonly IConfigProvider _configProvider = Substitute.For<IConfigProvider>();
    private readonly IScheduleTracker _scheduleTracker = Substitute.For<IScheduleTracker>();
    private readonly IEpisodeSelector _episodeSelector = Substitute.For<IEpisodeSelector>();
    private readonly IPlaylistInterleaver _interleaver = Substitute.For<IPlaylistInterleaver>();
    private readonly ISpotifyClient _spotifyClient = Substitute.For<ISpotifyClient>();
    private readonly ILogger<PlaylistOrchestrator> _logger = Substitute.For<ILogger<PlaylistOrchestrator>>();

    private PlaylistOrchestrator CreateOrchestrator() => new(
        _configProvider, _scheduleTracker, _episodeSelector,
        _interleaver, _spotifyClient, _logger);

    private static PlaylistConfig MakePlaylist(string id = "playlist1", params PodcastConfig[] podcasts)
    {
        return new PlaylistConfig
        {
            PlaylistId = id,
            Schedule = "0 * * * *",
            Podcasts = podcasts.Length > 0 ? podcasts.ToList() : new List<PodcastConfig>
            {
                new() { ShowId = "show1", Name = "Podcast A", Priority = 1, MaxEpisodes = 10 },
                new() { ShowId = "show2", Name = "Podcast B", Priority = 2, MaxEpisodes = 5 },
            }
        };
    }

    [Fact]
    public async Task FullPipeline_SelectsInterleavesAndReplacesPlaylist()
    {
        var playlist = MakePlaylist();
        _configProvider.GetPlaylists().Returns(new List<PlaylistConfig> { playlist });
        _scheduleTracker.IsDueAsync("playlist1", Arg.Any<string>()).Returns(true);

        var groupA = new PodcastEpisodeGroup { Priority = 1, EpisodeUris = new List<string> { "a1", "a2" } };
        var groupB = new PodcastEpisodeGroup { Priority = 2, EpisodeUris = new List<string> { "b1" } };
        _episodeSelector.SelectEpisodesAsync(playlist.Podcasts[0]).Returns(groupA);
        _episodeSelector.SelectEpisodesAsync(playlist.Podcasts[1]).Returns(groupB);

        var interleavedUris = new List<string> { "a1", "a2", "b1" };
        _interleaver.Interleave(Arg.Any<List<PodcastEpisodeGroup>>()).Returns(interleavedUris);

        await CreateOrchestrator().RunAsync();

        await _spotifyClient.Received(1).ReplacePlaylistTracksAsync("playlist1", interleavedUris);
        await _scheduleTracker.Received(1).RecordUpdateAsync("playlist1");
    }

    [Fact]
    public async Task PlaylistNotDue_IsSkipped()
    {
        _configProvider.GetPlaylists().Returns(new List<PlaylistConfig> { MakePlaylist() });
        _scheduleTracker.IsDueAsync("playlist1", Arg.Any<string>()).Returns(false);

        await CreateOrchestrator().RunAsync();

        await _episodeSelector.DidNotReceive().SelectEpisodesAsync(Arg.Any<PodcastConfig>());
        await _spotifyClient.DidNotReceive().ReplacePlaylistTracksAsync(Arg.Any<string>(), Arg.Any<List<string>>());
    }

    [Fact]
    public async Task PartialPodcastFailure_StillUpdatesPlaylist()
    {
        var playlist = MakePlaylist();
        _configProvider.GetPlaylists().Returns(new List<PlaylistConfig> { playlist });
        _scheduleTracker.IsDueAsync("playlist1", Arg.Any<string>()).Returns(true);

        // First podcast fails
        _episodeSelector.SelectEpisodesAsync(playlist.Podcasts[0])
            .Throws(new Exception("API error"));
        // Second podcast succeeds
        var groupB = new PodcastEpisodeGroup { Priority = 2, EpisodeUris = new List<string> { "b1" } };
        _episodeSelector.SelectEpisodesAsync(playlist.Podcasts[1]).Returns(groupB);

        _interleaver.Interleave(Arg.Any<List<PodcastEpisodeGroup>>()).Returns(new List<string> { "b1" });

        await CreateOrchestrator().RunAsync();

        await _spotifyClient.Received(1).ReplacePlaylistTracksAsync("playlist1", Arg.Any<List<string>>());
        await _scheduleTracker.Received(1).RecordUpdateAsync("playlist1");
    }

    [Fact]
    public async Task PlaylistUpdateFailure_DoesNotRecordUpdate()
    {
        _configProvider.GetPlaylists().Returns(new List<PlaylistConfig> { MakePlaylist() });
        _scheduleTracker.IsDueAsync("playlist1", Arg.Any<string>()).Returns(true);

        var group = new PodcastEpisodeGroup { Priority = 1, EpisodeUris = new List<string> { "a1" } };
        _episodeSelector.SelectEpisodesAsync(Arg.Any<PodcastConfig>()).Returns(group);
        _interleaver.Interleave(Arg.Any<List<PodcastEpisodeGroup>>()).Returns(new List<string> { "a1" });

        _spotifyClient.ReplacePlaylistTracksAsync("playlist1", Arg.Any<List<string>>())
            .Throws(new Exception("Spotify API error"));

        await CreateOrchestrator().RunAsync();

        await _scheduleTracker.DidNotReceive().RecordUpdateAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task MultiplePlaylists_EachEvaluatedIndependently()
    {
        var playlist1 = MakePlaylist("p1");
        var playlist2 = MakePlaylist("p2");
        _configProvider.GetPlaylists().Returns(new List<PlaylistConfig> { playlist1, playlist2 });

        _scheduleTracker.IsDueAsync("p1", Arg.Any<string>()).Returns(true);
        _scheduleTracker.IsDueAsync("p2", Arg.Any<string>()).Returns(false);

        var group = new PodcastEpisodeGroup { Priority = 1, EpisodeUris = new List<string> { "a1" } };
        _episodeSelector.SelectEpisodesAsync(Arg.Any<PodcastConfig>()).Returns(group);
        _interleaver.Interleave(Arg.Any<List<PodcastEpisodeGroup>>()).Returns(new List<string> { "a1" });

        await CreateOrchestrator().RunAsync();

        await _spotifyClient.Received(1).ReplacePlaylistTracksAsync("p1", Arg.Any<List<string>>());
        await _spotifyClient.DidNotReceive().ReplacePlaylistTracksAsync("p2", Arg.Any<List<string>>());
    }
}
