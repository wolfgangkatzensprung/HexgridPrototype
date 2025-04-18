﻿using System;
using UnityEngine;

public static class HexUtils
{
    public static int GetLength(Hex hex) => (Math.Abs(hex.Q) + Math.Abs(hex.R) + Math.Abs(hex.S)) / 2;

    public static int GetDistance(Hex a, Hex b) => GetLength(a - b);

    private static Hex[] directions = new Hex[6]
    {
        new Hex(0, +1, -1), // top
        new Hex(+1, 0, -1),
        new Hex(+1, -1, 0),
        new Hex(0, -1, +1), // bottom 
        new Hex(-1, 0, +1),
        new Hex(-1, +1, 0),
    };

    public enum DirectionType
    {
        Top = 0,
        TopRight = 1,
        BottomRight = 2,
        Bottom = 3,
        BottomLeft = 4,
        TopLeft = 5
    }

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

        Debug.LogError($"Invalid Shared Edge for {self} and {neighbor}");
        return -1;  // invalid
    }

    public static Hex GetDirectionToNeighbor(Hex self, Hex neighbor)
    {
        int i = GetSharedEdgeIndex(self, neighbor);
        return GetDirection(i);
    }

    /// <summary>
    /// Get all neighbour hex positions of a given hex
    /// </summary>
    public static Hex[] GetAllNeighbours(Hex hex)
    {
        var neighbors = new Hex[6];

        for (int i = 0; i < neighbors.Length; i++)
        {
            var dir = Directions[i];
            Hex neighbor = new Hex(hex.Q + dir.Q, hex.R + dir.R);
            neighbors[i] = neighbor;
        }

        return neighbors;
    }
}