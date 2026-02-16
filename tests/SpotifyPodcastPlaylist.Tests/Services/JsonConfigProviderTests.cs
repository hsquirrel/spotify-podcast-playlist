using NSubstitute;
using SpotifyPodcastPlaylist.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SpotifyPodcastPlaylist.Tests.Services;

public class JsonConfigProviderTests : IDisposable
{
    private readonly ILogger<JsonConfigProvider> _logger = Substitute.For<ILogger<JsonConfigProvider>>();
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }

    private JsonConfigProvider CreateProvider(string json)
    {
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);
        File.WriteAllText(path, json);
        return new JsonConfigProvider(_logger, path);
    }

    private static string ValidJson(string? overridePodcast = null)
    {
        var podcast = overridePodcast ?? """
            {
                "showId": "show1",
                "name": "Test Podcast",
                "priority": 2,
                "maxEpisodes": 10,
                "episodeOrder": "oldestFirst"
            }
            """;

        return $$"""
            {
                "playlists": [
                    {
                        "playlistId": "playlist1",
                        "schedule": "0 * * * *",
                        "podcasts": [{{podcast}}]
                    }
                ]
            }
            """;
    }

    [Fact]
    public void ValidConfig_ParsesSuccessfully()
    {
        var provider = CreateProvider(ValidJson());

        var playlists = provider.GetPlaylists();

        Assert.Single(playlists);
        Assert.Equal("playlist1", playlists[0].PlaylistId);
        Assert.Equal("0 * * * *", playlists[0].Schedule);
        Assert.Single(playlists[0].Podcasts);
        Assert.Equal("show1", playlists[0].Podcasts[0].ShowId);
        Assert.Equal("Test Podcast", playlists[0].Podcasts[0].Name);
        Assert.Equal(2, playlists[0].Podcasts[0].Priority);
        Assert.Equal(10, playlists[0].Podcasts[0].MaxEpisodes);
        Assert.Equal("oldestFirst", playlists[0].Podcasts[0].EpisodeOrder);
    }

    [Fact]
    public void MissingPlaylistId_Throws()
    {
        var json = """
            {
                "playlists": [{
                    "playlistId": "",
                    "schedule": "0 * * * *",
                    "podcasts": [{ "showId": "s1", "name": "P", "priority": 1, "maxEpisodes": 1 }]
                }]
            }
            """;
        var provider = CreateProvider(json);

        Assert.Throws<InvalidOperationException>(() => provider.GetPlaylists());
    }

    [Fact]
    public void EmptyPodcasts_Throws()
    {
        var json = """
            {
                "playlists": [{
                    "playlistId": "p1",
                    "schedule": "0 * * * *",
                    "podcasts": []
                }]
            }
            """;
        var provider = CreateProvider(json);

        Assert.Throws<InvalidOperationException>(() => provider.GetPlaylists());
    }

    [Fact]
    public void PriorityOutOfRange_Throws()
    {
        var provider = CreateProvider(ValidJson("""
            { "showId": "s1", "name": "P", "priority": 0, "maxEpisodes": 1 }
            """));

        Assert.Throws<InvalidOperationException>(() => provider.GetPlaylists());
    }

    [Fact]
    public void InvalidCronExpression_Throws()
    {
        var json = """
            {
                "playlists": [{
                    "playlistId": "p1",
                    "schedule": "not a cron",
                    "podcasts": [{ "showId": "s1", "name": "P", "priority": 1, "maxEpisodes": 1 }]
                }]
            }
            """;
        var provider = CreateProvider(json);

        Assert.Throws<InvalidOperationException>(() => provider.GetPlaylists());
    }

    [Fact]
    public void InvalidRegex_Throws()
    {
        var provider = CreateProvider(ValidJson("""
            { "showId": "s1", "name": "P", "priority": 1, "maxEpisodes": 1, "titleInclude": "[invalid" }
            """));

        Assert.Throws<InvalidOperationException>(() => provider.GetPlaylists());
    }
}
