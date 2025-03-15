using UnityEngine;
using System.Collections.Generic;

public class Level : Singleton<Level>
{
    [SerializeField] HexGrid hexGrid;
    public bool IsAreaCompleted(Hex hex, EdgeType type)
    {
        if (!IsValidEdgeType(type) || !IsHexValid(hex)) return false;

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
                if (!IsHexValid(neighborHex)) continue;
                if (currentHex == neighborHex) continue;

                if (!IsEdgeMatchingType(currentHex, neighborHex, type)) return false;

                if (!visited.Contains(neighborHex))
                {
                    toVisit.Enqueue(neighborHex);
                }
            }

            if (!hexGrid.GetTile(currentHex).IsClosed(type)) return false;
        }

        return true;
    }

    private bool IsValidEdgeType(EdgeType type)
    {
        return type != EdgeType.None;
    }

    private bool IsHexValid(Hex hex)
    {
        return hexGrid.Board.ContainsKey(hex);
    }

    private bool IsEdgeMatchingType(Hex currentHex, Hex neighborHex, EdgeType type)
    {
        Tile tile = hexGrid.Board[currentHex];
        Tile neighborTile = hexGrid.Board[neighborHex];
        var sharedEdgeIndex = HexUtils.GetSharedEdgeIndex(currentHex, neighborHex);
        return tile.Matches(neighborTile, sharedEdgeIndex);
    }

}