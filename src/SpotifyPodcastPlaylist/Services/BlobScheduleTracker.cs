using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SpotifyPodcastPlaylist.Services;

public class BlobScheduleTracker : IScheduleTracker
{
    private const string ContainerName = "playlist-schedules";
    private const string BlobName = "last-updates.json";

    private readonly IConfiguration _configuration;
    private readonly ILogger<BlobScheduleTracker> _logger;

    public BlobScheduleTracker(IConfiguration configuration, ILogger<BlobScheduleTracker> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsDueAsync(string playlistId, string cronExpression)
    {
        var timestamps = await LoadTimestampsAsync();

        if (!timestamps.TryGetValue(playlistId, out var lastUpdate))
        {
            _logger.LogInformation("Playlist {PlaylistId} has no prior update, marking as due", playlistId);
            return true;
        }

        var cron = CronExpression.Parse(cronExpression);
        var occurrences = cron.GetOccurrences(lastUpdate, DateTime.UtcNow);

        var isDue = occurrences.Any();
        _logger.LogInformation("Playlist {PlaylistId} is {Status}. Last update: {LastUpdate}",
            playlistId, isDue ? "due" : "not due", lastUpdate);

        return isDue;
    }

    public async Task RecordUpdateAsync(string playlistId)
    {
        var timestamps = await LoadTimestampsAsync();
        timestamps[playlistId] = DateTime.UtcNow;

        var blobClient = GetBlobClient();
        var json = JsonSerializer.Serialize(timestamps);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        await blobClient.UploadAsync(stream, overwrite: true);

        _logger.LogInformation("Recorded update time for playlist {PlaylistId}", playlistId);
    }

    private async Task<Dictionary<string, DateTime>> LoadTimestampsAsync()
    {
        var blobClient = GetBlobClient();

        try
        {
            var response = await blobClient.DownloadContentAsync();
            var json = response.Value.Content.ToString();
            return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json)
                ?? new Dictionary<string, DateTime>();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new Dictionary<string, DateTime>();
        }
    }

    private BlobClient GetBlobClient()
    {
        var connectionString = _configuration["AzureWebJobsStorage"]
            ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured");

        var serviceClient = new BlobServiceClient(connectionString);
        var containerClient = serviceClient.GetBlobContainerClient(ContainerName);
        containerClient.CreateIfNotExists();
        return containerClient.GetBlobClient(BlobName);
    }
}
