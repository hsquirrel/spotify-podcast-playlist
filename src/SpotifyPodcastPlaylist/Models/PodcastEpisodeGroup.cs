namespace SpotifyPodcastPlaylist.Models;

public class PodcastEpisodeGroup
{
    public int Priority { get; set; }

    public List<string> EpisodeUris { get; set; } = new();
}
