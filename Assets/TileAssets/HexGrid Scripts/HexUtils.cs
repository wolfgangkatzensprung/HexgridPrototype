using System.Collections.Generic;
using System;
using System.Diagnostics;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class HexUtils
{
    public static int GetLength(Hex hex) => (Math.Abs(hex.Q) + Math.Abs(hex.R) + Math.Abs(hex.S)) / 2;

    public static int GetDistance(Hex a, Hex b) => GetLength(a - b);

    private static Hex[] directions = new Hex[6]
    {
        new Hex(0, -1, +1), // top 
        new Hex(-1, 0, +1), 
        new Hex(-1, +1, 0), 
        new Hex(0, +1, -1), // bottom
        new Hex(+1, 0, -1), 
        new Hex(+1, -1, 0)  
    };

    public static Hex[] Directions => directions;

    public static Hex GetDirection(int direction) => directions[(direction % 6 + 6) % 6];   // usually use 0...5 but wraps around to work for all numbers

    public static Hex GetNeighbour(Hex hex, int direction) => hex + directions[direction];

    public static int GetSharedEdgeIndex(Hex self, Hex neighbor)
    {
        var edgeDir = neighbor - self;

        for (int i = 0; i < directions.Length; i++)
        {
            if (edgeDir == directions[i])
            {
                return i;
            }
        }

        return -1;  // invalid
    }

    public static Hex GetDirectionToNeighbor(Hex self, Hex neighbor)
    {
        int i = HexUtils.GetSharedEdgeIndex(self, neighbor);
        return GetDirection(i);
    }
}