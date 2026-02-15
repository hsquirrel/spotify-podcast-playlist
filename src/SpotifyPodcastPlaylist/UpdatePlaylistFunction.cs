using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SpotifyPodcastPlaylist;

public class UpdatePlaylistFunction
{
    private readonly ILogger<UpdatePlaylistFunction> _logger;

    public UpdatePlaylistFunction(ILogger<UpdatePlaylistFunction> logger)
    {
        _logger = logger;
    }

    [Function("UpdatePlaylist")]
    public void Run([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("UpdatePlaylist function executed at: {Time}", DateTime.UtcNow);

        if (timerInfo.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {Next}", timerInfo.ScheduleStatus.Next);
        }
    }
}
