using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class SimpleA
{
    struct Node
    {
        public Vector3Int cell;
        public int g; 
        public int h;     
        public int f => g + h;
        public Vector3Int parent;
        public bool hasParent;
    }

    static readonly Vector3Int[] Neigh4 = new[]
    {
        new Vector3Int( 1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int( 0, 1, 0),
        new Vector3Int( 0,-1, 0),
    };

    public static List<Vector3> FindPath(Vector3 worldStart, Vector3 worldEnd, Tilemap obstacle)
    {
        if (obstacle == null) return new List<Vector3>() { worldEnd };

        Vector3Int start = obstacle.WorldToCell(worldStart);
        Vector3Int goal = obstacle.WorldToCell(worldEnd);

        if (!IsWalkable(goal, obstacle))
        {
            Vector3Int alt = FindNearestWalkable(goal, obstacle, 8);
            if (alt != goal) goal = alt;
        }

        var open = new List<Node>();
        var came = new Dictionary<Vector3Int, Node>();
        var closed = new HashSet<Vector3Int>();

        Node startNode = new Node { cell = start, g = 0, h = Heuristic(start, goal), hasParent = false };
        open.Add(startNode);
        came[start] = startNode;

        int safety = 5000; 
        while (open.Count > 0 && safety-- > 0)
        {

            int bestIdx = 0;
            for (int i = 1; i < open.Count; i++)
                if (open[i].f < open[bestIdx].f) bestIdx = i;

            Node cur = open[bestIdx];
            open.RemoveAt(bestIdx);
            closed.Add(cur.cell);

            if (cur.cell == goal)
            {
                return Reconstruct(came, cur, obstacle);
            }

            foreach (var d in Neigh4)
            {
                Vector3Int nb = cur.cell + d;
                if (closed.Contains(nb)) continue;
                if (!IsWalkable(nb, obstacle)) continue;

                int tentativeG = cur.g + 10; 
                bool inOpen = came.ContainsKey(nb);
                if (!inOpen || tentativeG < came[nb].g)
                {
                    Node nn = new Node
                    {
                        cell = nb,
                        g = tentativeG,
                        h = Heuristic(nb, goal),
                        parent = cur.cell,
                        hasParent = true
                    };
                    came[nb] = nn;
                    if (!inOpen) open.Add(nn);
                }
            }
        }

        return new List<Vector3>();
    }

    static int Heuristic(Vector3Int a, Vector3Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 10;
    }

    static List<Vector3> Reconstruct(Dictionary<Vector3Int, Node> came, Node end, Tilemap obstacle)
    {
        var cells = new List<Vector3Int>();
        Node cur = end;
        cells.Add(cur.cell);
        while (cur.hasParent && came.TryGetValue(cur.parent, out var parent))
        {
            cur = parent;
            cells.Add(cur.cell);
        }
        cells.Reverse();

        var path = new List<Vector3>(cells.Count);
        for (int i = 0; i < cells.Count; i++)
            path.Add(obstacle.GetCellCenterWorld(cells[i]));
        return path;
    }

    static bool IsWalkable(Vector3Int cell, Tilemap obstacle)
    {
        return !obstacle.HasTile(cell);
    }

    static Vector3Int FindNearestWalkable(Vector3Int from, Tilemap obstacle, int maxRadius)
    {
        if (IsWalkable(from, obstacle)) return from;
        for (int r = 1; r <= maxRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                int y1 = r;
                int y2 = -r;
                var c1 = new Vector3Int(from.x + x, from.y + y1, 0);
                var c2 = new Vector3Int(from.x + x, from.y + y2, 0);
                if (IsWalkable(c1, obstacle)) return c1;
                if (IsWalkable(c2, obstacle)) return c2;
            }
            for (int y = -r + 1; y <= r - 1; y++)
            {
                int x1 = r;
                int x2 = -r;
                var c1 = new Vector3Int(from.x + x1, from.y + y, 0);
                var c2 = new Vector3Int(from.x + x2, from.y + y, 0);
                if (IsWalkable(c1, obstacle)) return c1;
                if (IsWalkable(c2, obstacle)) return c2;
            }
        }
        return from;
    }
}
