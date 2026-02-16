using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

public class PlaylistInterleaver : IPlaylistInterleaver
{
    public List<string> Interleave(List<PodcastEpisodeGroup> groups)
    {
        var result = new List<string>();

        var nonEmpty = groups.Where(g => g.EpisodeUris.Count > 0).ToList();
        if (nonEmpty.Count == 0)
            return result;

        var priorityGroups = nonEmpty
            .GroupBy(g => g.Priority)
            .OrderBy(pg => pg.Key);

        foreach (var priorityGroup in priorityGroups)
        {
            var queues = priorityGroup
                .Select(g => new Queue<string>(g.EpisodeUris))
                .ToList();

            while (queues.Any(q => q.Count > 0))
            {
                foreach (var queue in queues)
                {
                    if (queue.Count > 0)
                        result.Add(queue.Dequeue());
                }
            }
        }

        return result;
    }
}
