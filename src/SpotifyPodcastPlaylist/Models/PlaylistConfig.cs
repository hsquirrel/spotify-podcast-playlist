using System.Text.Json.Serialization;

namespace SpotifyPodcastPlaylist.Models;

public class PlaylistConfig
{
    [JsonPropertyName("playlistId")]
    public string PlaylistId { get; set; } = string.Empty;

    [JsonPropertyName("schedule")]
    public string Schedule { get; set; } = string.Empty;

    [JsonPropertyName("podcasts")]
    public List<PodcastConfig> Podcasts { get; set; } = new();
}
