using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SpotifyPodcastPlaylist.Services;

namespace SpotifyPodcastPlaylist.Functions;

public class UpdatePlaylistFunction
{
    private readonly PlaylistOrchestrator _orchestrator;
    private readonly ILogger<UpdatePlaylistFunction> _logger;

    public UpdatePlaylistFunction(PlaylistOrchestrator orchestrator, ILogger<UpdatePlaylistFunction> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [Function("UpdatePlaylist")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("UpdatePlaylist function executed at: {Time}", DateTime.UtcNow);

        try
        {
            await _orchestrator.RunAsync();
            _logger.LogInformation("UpdatePlaylist completed at: {Time}", DateTime.UtcNow);
        }
        catch (SpotifyAPI.Web.APIUnauthorizedException ex)
        {
            _logger.LogError(ex, "Unauthorized — refresh token may be invalid. Skipping run.");
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePlaylist failed: {Message}", ex.Message);
            throw;
        }

        if (timerInfo.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {Next}", timerInfo.ScheduleStatus.Next);
        }
    }
}
