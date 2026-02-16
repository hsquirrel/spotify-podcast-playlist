using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace SpotifyPodcastPlaylist.Services;

public class SpotifyClientWrapper : ISpotifyClient
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpotifyClientWrapper> _logger;
    private SpotifyClient? _client;

    public SpotifyClientWrapper(IConfiguration configuration, ILogger<SpotifyClientWrapper> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<SimpleEpisode>> GetShowEpisodesAsync(string showId, int maxPages = 0)
    {
        var client = await GetClientAsync();

        try
        {
            var firstPage = await client.Shows.GetEpisodes(showId, new ShowEpisodesRequest { Market = "US" });

            List<SimpleEpisode> allEpisodes;
            if (maxPages <= 0)
            {
                allEpisodes = (await client.PaginateAll(firstPage)).ToList();
            }
            else
            {
                allEpisodes = new List<SimpleEpisode>(firstPage.Items ?? Enumerable.Empty<SimpleEpisode>());
                var currentPage = firstPage;
                var pagesRead = 1;
                while (pagesRead < maxPages && currentPage.Next != null)
                {
                    currentPage = await client.NextPage(currentPage);
                    if (currentPage?.Items != null)
                        allEpisodes.AddRange(currentPage.Items);
                    pagesRead++;
                }
            }

            _logger.LogInformation("Fetched {Count} episodes for show {ShowId}", allEpisodes.Count, showId);
            return allEpisodes;
        }
        catch (APIException ex) when (ex.Response?.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Show {ShowId} not found (404), returning empty list", showId);
            return new List<SimpleEpisode>();
        }
        catch (APIException ex) when (ex.Response?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Unauthorized (401) when fetching show {ShowId}", showId);
            throw;
        }
        catch (APIException ex)
        {
            _logger.LogWarning(ex, "API error fetching show {ShowId}, returning empty list", showId);
            return new List<SimpleEpisode>();
        }
    }

    public async Task ReplacePlaylistTracksAsync(string playlistId, List<string> uris)
    {
        var client = await GetClientAsync();

        if (uris.Count == 0)
        {
            await client.Playlists.ReplaceItems(playlistId, new PlaylistReplaceItemsRequest(new List<string>()));
            _logger.LogInformation("Cleared playlist {PlaylistId}", playlistId);
            return;
        }

        // First batch: replace (clears existing + adds first 100)
        var firstBatch = uris.Take(100).ToList();
        await client.Playlists.ReplaceItems(playlistId, new PlaylistReplaceItemsRequest(firstBatch));

        // Remaining batches: add in chunks of 100
        var remaining = uris.Skip(100).ToList();
        for (var i = 0; i < remaining.Count; i += 100)
        {
            var chunk = remaining.Skip(i).Take(100).ToList();
            await client.Playlists.AddItems(playlistId, new PlaylistAddItemsRequest(chunk));
        }

        _logger.LogInformation("Replaced playlist {PlaylistId} with {Count} tracks", playlistId, uris.Count);
    }

    private Task<SpotifyClient> GetClientAsync()
    {
        if (_client is not null)
            return Task.FromResult(_client);

        var clientId = _configuration["Spotify:ClientId"]
            ?? throw new InvalidOperationException("Spotify:ClientId is not configured");
        var clientSecret = _configuration["Spotify:ClientSecret"]
            ?? throw new InvalidOperationException("Spotify:ClientSecret is not configured");
        var refreshToken = _configuration["Spotify:RefreshToken"]
            ?? throw new InvalidOperationException("Spotify:RefreshToken is not configured");

        var tokenResponse = new AuthorizationCodeTokenResponse { RefreshToken = refreshToken };
        var authenticator = new AuthorizationCodeAuthenticator(clientId, clientSecret, tokenResponse);

        var config = SpotifyClientConfig.CreateDefault()
            .WithRetryHandler(new SimpleRetryHandler
            {
                RetryAfter = TimeSpan.FromSeconds(1),
                RetryTimes = 5,
                TooManyRequestsConsumesARetry = false
            })
            .WithAuthenticator(authenticator);

        _client = new SpotifyClient(config);
        _logger.LogInformation("Spotify client initialized");

        return Task.FromResult(_client);
    }
}
