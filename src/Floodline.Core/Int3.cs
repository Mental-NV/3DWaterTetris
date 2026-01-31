namespace Floodline.Core;

public readonly record struct Int3(int X, int Y, int Z)
{
    public static Int3 Zero => new(0, 0, 0);

    public static Int3 operator +(Int3 a, Int3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Int3 operator -(Int3 a, Int3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Int3 operator *(Int3 a, int b) => new(a.X * b, a.Y * b, a.Z * b);
    public static Int3 operator -(Int3 a) => new(-a.X, -a.Y, -a.Z);

    public int Dot(Int3 other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);

    public Int3 Cross(Int3 other) => new(
        (Y * other.Z) - (Z * other.Y),
        (Z * other.X) - (X * other.Z),
        (X * other.Y) - (Y * other.X)
    );

    public override string ToString() => $"({X}, {Y}, {Z})";
}
