namespace SpotifyPodcastPlaylist.Services;

/// <summary>
/// Tracks playlist update schedules using stored timestamps and cron evaluation.
/// </summary>
public interface IScheduleTracker
{
    /// <summary>
    /// Determines whether a playlist is due for an update by comparing its cron schedule
    /// against the last recorded update time. Returns true on first run (no stored timestamp).
    /// See tech spec section 4.
    /// </summary>
    Task<bool> IsDueAsync(string playlistId, string cronExpression);

    /// <summary>
    /// Records the current UTC time as the last update timestamp for a playlist.
    /// </summary>
    Task RecordUpdateAsync(string playlistId);
}
