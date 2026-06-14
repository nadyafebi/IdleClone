using System.Collections.Generic;
using UnityEngine;

public static class PlatformPathfinder
{
    public static List<PlatformNode> FindPath(PlatformNode start, PlatformNode goal)
    {
        if (start == null || goal == null)
        {
            Debug.LogWarning("[Pathfinder] FindPath called with a null start or goal node.");
            return new List<PlatformNode>();
        }

        if (start == goal)
            return new List<PlatformNode> { start };

        // BFS
        var cameFrom = new Dictionary<PlatformNode, PlatformNode>();
        var queue = new Queue<PlatformNode>();

        queue.Enqueue(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            PlatformNode current = queue.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, goal);

            foreach (PlatformNode neighbor in current.neighbors)
            {
                if (cameFrom.ContainsKey(neighbor))
                    continue;

                cameFrom[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        Debug.LogWarning(
            $"[Pathfinder] No path found from {start.worldPosition} to {goal.worldPosition}."
        );
        return new List<PlatformNode>();
    }

    #region Private Helpers

    static List<PlatformNode> ReconstructPath(
        Dictionary<PlatformNode, PlatformNode> cameFrom,
        PlatformNode goal
    )
    {
        var path = new List<PlatformNode>();
        PlatformNode current = goal;

        while (current != null)
        {
            path.Add(current);
            cameFrom.TryGetValue(current, out current);
        }

        path.Reverse();
        return path;
    }

    #endregion
}
