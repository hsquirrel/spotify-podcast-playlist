using System.Text.Json;
using Azure.Storage.Blobs;
using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SpotifyPodcastPlaylist.Services;

public class BlobScheduleTracker : IScheduleTracker
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlobScheduleTracker> _logger;

    public BlobScheduleTracker(IConfiguration configuration, ILogger<BlobScheduleTracker> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> IsDueAsync(string playlistId, string cronExpression)
    {
        // TODO: Phase 1 — Schedule evaluation (tech spec section 4)
        //
        // 1. Load the last update timestamps from Azure Blob Storage:
        //    a. Get connection string from _configuration["AzureWebJobsStorage"]
        //    b. Create BlobServiceClient, get container "playlist-schedules"
        //    c. Get blob "last-updates.json"
        //    d. If blob doesn't exist, this is first run — return true
        //    e. Download and deserialize to Dictionary<string, DateTime>
        //
        // 2. Look up this playlist's last update time:
        //    - If no entry for playlistId, this is first run — return true
        //    - var lastUpdate = timestamps[playlistId]
        //
        // 3. Parse the cron expression and find the most recent scheduled time:
        //    - var cron = CronExpression.Parse(cronExpression)
        //    - var mostRecentDue = cron.GetOccurrences(lastUpdate, DateTime.UtcNow).LastOrDefault()
        //    - Or: iterate backwards from now to find when it was last due
        //
        // 4. If mostRecentDue is after lastUpdate, the playlist is due — return true
        //    Otherwise return false
        //
        // 5. Log the decision: "Playlist {playlistId} is {due/not due}. Last update: {lastUpdate}"

        throw new NotImplementedException();
    }

    public async Task RecordUpdateAsync(string playlistId)
    {
        // TODO: Phase 1 — Schedule tracking (tech spec section 4)
        //
        // 1. Load existing timestamps from blob (same as IsDueAsync step 1)
        //    - If blob doesn't exist, start with empty dictionary
        //
        // 2. Set timestamps[playlistId] = DateTime.UtcNow
        //
        // 3. Serialize the dictionary back to JSON
        //
        // 4. Upload to blob (overwrite):
        //    a. Get BlobClient for "playlist-schedules/last-updates.json"
        //    b. Upload with overwrite: true
        //
        // 5. Log: "Recorded update time for playlist {playlistId}"

        throw new NotImplementedException();
    }
}
