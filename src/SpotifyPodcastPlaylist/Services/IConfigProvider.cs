using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

/// <summary>
/// Provides access to playlist configuration data.
/// </summary>
public interface IConfigProvider
{
    /// <summary>
    /// Loads and returns all configured playlists.
    /// Throws if configuration is missing or invalid.
    /// </summary>
    List<PlaylistConfig> GetPlaylists();
}
