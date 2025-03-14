using UnityEngine;
using System.Collections.Generic;

public class Level : Singleton<Level>
{
    [SerializeField] HexGrid hexGrid;

    // Breadth-First-Search
    public bool IsAreaCompleted(Hex hex, EdgeType type)
    {
        if (type == EdgeType.None) return false;
        if (!hexGrid.Board.ContainsKey(hex)) return false;

        Debug.Log($"Check Area Completion for {type}");

        HashSet<Hex> visited = new();
        Queue<Hex> toVisit = new();
        toVisit.Enqueue(hex);

        while (toVisit.Count > 0)
        {
            Hex currentHex = toVisit.Dequeue();

            if (visited.Contains(currentHex)) continue;
            visited.Add(currentHex);

            var neighborHexes = hexGrid.GetOccupiedNeighbours(currentHex);

            foreach (var neighborHex in neighborHexes)
            {
                if (!hexGrid.Board.ContainsKey(neighborHex)) continue;
                if (currentHex == neighborHex) continue;

                Tile tile = hexGrid.Board[currentHex];
                Tile neighborTile = hexGrid.Board[neighborHex];

                var sharedEdgeIndex = HexUtils.GetSharedEdgeIndex(currentHex, neighborHex);

                if (neighborTile.Edges[sharedEdgeIndex].Type != type) return false;

                if (!visited.Contains(neighborHex))
                {
                    toVisit.Enqueue(neighborHex);
                }
            }
        }

        return true;
    }
}