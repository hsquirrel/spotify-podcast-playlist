using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifyPodcastPlaylist.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // TODO: Phase 1 — DI registration
        //
        // Register all services in the DI container:
        // - IConfigProvider -> JsonConfigProvider (singleton — config is loaded once)
        // - IScheduleTracker -> BlobScheduleTracker (singleton — reuses blob client)
        // - ISpotifyClient -> SpotifyClientWrapper (singleton — reuses authenticated client)
        // - IEpisodeSelector -> EpisodeSelector (transient)
        // - IPlaylistInterleaver -> PlaylistInterleaver (transient)
        // - PlaylistOrchestrator (transient)
        //
        // Example:
        // services.AddSingleton<IConfigProvider, JsonConfigProvider>();
        // services.AddSingleton<IScheduleTracker, BlobScheduleTracker>();
        // services.AddSingleton<ISpotifyClient, SpotifyClientWrapper>();
        // services.AddTransient<IEpisodeSelector, EpisodeSelector>();
        // services.AddTransient<IPlaylistInterleaver, PlaylistInterleaver>();
        // services.AddTransient<PlaylistOrchestrator>();
    })
    .Build();

host.Run();
