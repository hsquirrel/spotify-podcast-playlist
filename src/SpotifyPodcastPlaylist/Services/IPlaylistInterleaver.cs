using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

/// <summary>
/// Interleaves episode groups into a single ordered playlist using priority-based grouping.
/// </summary>
public interface IPlaylistInterleaver
{
    /// <summary>
    /// Takes a list of podcast episode groups (each with a priority and ordered episode URIs)
    /// and produces a single flat list of URIs ordered by priority with round-robin interleaving
    /// within each priority group. See tech spec section 5.
    /// </summary>
    List<string> Interleave(List<PodcastEpisodeGroup> groups);
}
