using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct HexVector
{
    public readonly int x;
    public readonly int y;

    public readonly int q;
    public readonly int r;
    public readonly int s;


    public HexVector(int x, int y)
    {
        this.x = x;
        this.y = y;

        q = x;
        r = y;
        s = -q - r;
    }

    private HexVector(int q, int r, int s)
    {
        x = q;
        y = r;

        this.q = q;
        this.r = r;
        this.s = s;
    }


    public override bool Equals(object obj) => obj is HexVector other && Equals(other);
    public bool Equals(HexVector other) => x == other.x && y == other.y;

    public override int GetHashCode() => (x, y).GetHashCode();

    public override string ToString() => new Vector2Int(x, y).ToString();

    public static bool operator ==(HexVector a, HexVector b) => a.Equals(b);
    public static bool operator !=(HexVector a, HexVector b) => !a.Equals(b);


    public int Length => (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2;


    public static HexVector operator +(HexVector v) => v;
    public static HexVector operator +(HexVector a, HexVector b) => new HexVector(a.q + b.q, a.r + b.r, a.s + b.s);

    public static HexVector operator -(HexVector v) => new HexVector(-v.q, -v.r, -v.s);
    public static HexVector operator -(HexVector a, HexVector b) => new HexVector(a.q - b.q, a.r - b.r, a.s - b.s);

    public static HexVector operator *(int s, HexVector v) => new HexVector(s * v.q, s * v.r, s * v.s);
    public static HexVector operator *(HexVector v, int s) => new HexVector(s * v.q, s * v.r, s * v.s);


    public static HexVector Zero => new HexVector(0, 0, 0);
    public static HexVector Up => new HexVector(0, -1, 1);
    public static HexVector UpRight => new HexVector(1, -1, 0);
    public static HexVector UpLeft => new HexVector(-1, 0, 1);
    public static HexVector Down => new HexVector(0, 1, -1);
    public static HexVector DownRight => new HexVector(1, 0, -1);
    public static HexVector DownLeft => new HexVector(-1, 1, 0);


    public static IEnumerable<HexVector> Ring(int radius)
    {
        if (radius < 0)
            yield break;

        if (radius == 0)
        {
            yield return Zero;
        }
        else
        {
            for (int i = 0; i < radius; i++)
                yield return Up * radius + DownLeft * i;
            for (int i = 0; i < radius; i++)
                yield return UpLeft * radius + Down * i;
            for (int i = 0; i < radius; i++)
                yield return DownLeft * radius + DownRight * i;
            for (int i = 0; i < radius; i++)
                yield return Down * radius + UpRight * i;
            for (int i = 0; i < radius; i++)
                yield return DownRight * radius + Up * i;
            for (int i = 0; i < radius; i++)
                yield return UpRight * radius + UpLeft * i;
        }
    }


    public static IEnumerable<HexVector> Spiral(int radius)
    {
        var result = Enumerable.Empty<HexVector>();
        for (int i = 0; i < radius; i++)
            result = result.Concat(Ring(i));

        return result;
    }
}
