using System.Text.Json.Serialization;

namespace SpotifyPodcastPlaylist.Models;

public class PodcastConfig
{
    [JsonPropertyName("showId")]
    public string ShowId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("maxEpisodes")]
    public int MaxEpisodes { get; set; }

    [JsonPropertyName("episodeOrder")]
    public string EpisodeOrder { get; set; } = "oldestFirst";

    [JsonPropertyName("maxLookbackDays")]
    public int? MaxLookbackDays { get; set; }

    [JsonPropertyName("titleInclude")]
    public string? TitleInclude { get; set; }

    [JsonPropertyName("titleExclude")]
    public string? TitleExclude { get; set; }
}
