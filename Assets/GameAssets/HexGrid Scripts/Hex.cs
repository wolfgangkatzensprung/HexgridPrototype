using UnityEngine;
using System;
using Unity.VisualScripting;
/// <summary>
/// This is a datastruct for Hexagons, similar to Vector3Int but more specific
/// </summary>
[Serializable, Inspectable]
public struct Hex
{
    [Inspectable]
    public int Q { get; }   // Column
    [Inspectable]
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

    public Hex(Vector2Int axialCoordinates) : this(axialCoordinates.x, axialCoordinates.y) { }

    public static implicit operator Vector2Int(Hex hex)
    {
        return new Vector2Int(hex.Q, hex.R);
    }

    public static explicit operator Hex(Vector2Int v)
    {
        return new Hex(v.x, v.y);
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
        return base.ToString() + s;
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

    public FractionalHex(float q, float r) : this(q, r, -q - r) { }

    // Convert from Hex to FractionalHex
    public FractionalHex(int q, int r, int s)
    {
        Q = q;
        R = r;
        S = s;
    }

    public float Q { get; }
    public float R { get; }
    public float S { get; }

    public static FractionalHex operator +(FractionalHex a, FractionalHex b) => new FractionalHex(a.Q + b.Q, a.R + b.R);
    public static FractionalHex operator *(FractionalHex a, float k) => new FractionalHex(a.Q * k, a.R * k, a.S * k);

    public static explicit operator FractionalHex(Hex hex)
    => new FractionalHex(hex.Q, hex.R, hex.S);
}