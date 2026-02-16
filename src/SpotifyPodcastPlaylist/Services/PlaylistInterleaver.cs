using SpotifyPodcastPlaylist.Models;

namespace SpotifyPodcastPlaylist.Services;

public class PlaylistInterleaver : IPlaylistInterleaver
{
    public List<string> Interleave(List<PodcastEpisodeGroup> groups)
    {
        // TODO: Phase 4 — Interleaving algorithm (tech spec section 5, Phase 2)
        //
        // 1. Filter out groups with empty EpisodeUris lists
        //
        // 2. Group the remaining PodcastEpisodeGroups by Priority level:
        //    - var priorityGroups = groups.GroupBy(g => g.Priority).OrderBy(g => g.Key)
        //
        // 3. For each priority group (ascending order — 1 first):
        //    a. Get all podcast groups at this priority level
        //    b. Create a queue (or index) for each group's EpisodeUris
        //    c. Round-robin interleave:
        //       - While any queue still has episodes:
        //         - For each podcast group (in config order, i.e., list order):
        //           - If it has remaining episodes, dequeue the next one and append to result
        //       - This produces: a1, b1, a2, b2, a3 (from the worked example)
        //
        // 4. Append the interleaved results from this priority group to the final list
        //
        // 5. Return the final flat list of episode URIs
        //
        // Edge cases (tech spec section 5):
        // - Empty queue: skip during round-robin
        // - Single podcast in group: append its episodes in order
        // - All empty: return empty list
        //
        // Worked example (tech spec section 5):
        // A(pri=1): [a1,a2,a3], B(pri=1): [b1,b2], C(pri=3): [c1,c2], D(pri=3): [d1]
        // Result: [a1, b1, a2, b2, a3, c1, d1, c2]

        throw new NotImplementedException();
    }
}
