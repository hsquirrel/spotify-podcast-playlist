using SpotifyAPI.Web;

namespace SpotifyPodcastPlaylist.Services;

/// <summary>
/// Abstraction over the Spotify Web API for podcast and playlist operations.
/// </summary>
public interface ISpotifyClient
{
    /// <summary>
    /// Fetches all episodes for a show, paginating through the API automatically.
    /// Returns episodes in the API's default order (newest first).
    /// Each episode includes resume_point data for fully-played detection.
    /// </summary>
    Task<List<SimpleEpisode>> GetShowEpisodesAsync(string showId);

    /// <summary>
    /// Replaces the entire contents of a playlist with the given track/episode URIs.
    /// Handles batching into chunks of 100 URIs per request (Spotify API limit).
    /// </summary>
    Task ReplacePlaylistTracksAsync(string playlistId, List<string> uris);
}
