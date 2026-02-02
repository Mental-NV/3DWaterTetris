namespace Floodline.Core;

public enum PieceId
{
    O2,
    I3,
    I4,
    L3,
    L4,
    J4,
    T3,
    S4,
    Z4,
    U5,
    P5,
    C3D5
}

public sealed record PieceDefinition(
    PieceId Id,
    IReadOnlyList<Int3> Voxels,
    IReadOnlyList<string> Tags)
{
    public IReadOnlyList<IReadOnlyList<Int3>> UniqueOrientations { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<Int3>> NormalizedOrientations { get; init; } = [];
}
