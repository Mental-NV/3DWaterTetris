namespace Floodline.Core;

public sealed record OrientedPiece(
    PieceId Id,
    IReadOnlyList<Int3> Voxels,
    int OrientationIndex);
