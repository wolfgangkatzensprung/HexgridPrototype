﻿using UnityEngine;
using System;
/// <summary>
/// This is a datastruct for Hexagons, similar to Vector3Int but more specific
/// </summary>
public struct Hex
{
    public int Q { get; }   // Column
    public int R { get; }   // Row
    public int S { get; }   // zusaetzliche diagonale Axe

    public Hex(int q, int r) : this(q, r, -q - r) { }

    public Hex(int q, int r, int s)
    {
        Q = q;
        R = r;
        S = s;
        Debug.Assert(q + r + s == 0);
    }

    public static Hex operator +(Hex a, Hex b) => new Hex(a.Q + b.Q, a.R + b.R);
    public static Hex operator -(Hex a, Hex b) => new Hex(a.Q - b.Q, a.R - b.R);
    public static Hex operator *(Hex a, int k) => new Hex(a.Q * k, a.R * k, a.S * k);

    public static bool operator ==(Hex a, Hex b) => a.Q == b.Q && a.R == b.R && a.S == b.S;
    public static bool operator !=(Hex a, Hex b) => !(a == b);

    public override bool Equals(object obj) => obj is Hex other && this == other;
    public override int GetHashCode() => HashCode.Combine(Q, R, S);

    public override string ToString()
    {
        var s = $"({Q}, {R}, {S})";
        return s;
    }

    public static Hex Round(float q, float r)
    {
        float s = -q - r;

        int qInt = Mathf.RoundToInt(q);
        int rInt = Mathf.RoundToInt(r);
        int sInt = Mathf.RoundToInt(s);

        float qDiff = Mathf.Abs(qInt - q);
        float rDiff = Mathf.Abs(rInt - r);
        float sDiff = Mathf.Abs(sInt - s);

        if (qDiff > rDiff && qDiff > sDiff)
            qInt = -rInt - sInt;
        else if (rDiff > sDiff)
            rInt = -qInt - sInt;

        return new Hex(qInt, rInt);
    }
}

public struct FractionalHex
{
    public FractionalHex(float q, float r, float s)
    {
        Q = q;
        R = r;
        S = s;
    }

    public float Q { get; }
    public float R { get; }
    public float S { get; }
}