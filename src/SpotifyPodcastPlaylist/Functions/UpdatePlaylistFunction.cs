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

        // TODO: Phase 5 — Function wiring (tech spec sections 1, 8)
        //
        // 1. Call the orchestrator:
        //    - await _orchestrator.RunAsync()
        //
        // 2. Error handling (tech spec section 8):
        //    - Wrap in try/catch
        //    - On configuration errors: log error, let it propagate (fail loudly)
        //    - On 401 Unauthorized: log error, skip run (don't crash the function)
        //    - On other runtime errors: log error, let Azure Functions retry on next tick
        //
        // 3. Log completion:
        //    - _logger.LogInformation("UpdatePlaylist completed at: {Time}", DateTime.UtcNow)

        if (timerInfo.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {Next}", timerInfo.ScheduleStatus.Next);
        }
    }
}
