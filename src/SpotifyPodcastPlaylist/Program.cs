using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifyPodcastPlaylist.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IConfigProvider, JsonConfigProvider>();
        services.AddSingleton<IScheduleTracker, BlobScheduleTracker>();
        services.AddSingleton<ISpotifyClient, SpotifyClientWrapper>();
        services.AddTransient<IEpisodeSelector, EpisodeSelector>();
        services.AddTransient<IPlaylistInterleaver, PlaylistInterleaver>();
        services.AddTransient<PlaylistOrchestrator>();
    })
    .Build();

host.Run();
