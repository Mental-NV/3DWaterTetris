namespace Floodline.Core.Movement;

/// <summary>
/// Defines the canonical kick table for piece rotation.
/// Per Content_Pack_v0_2 Section 3.
/// </summary>
public static class KickTable
{
    /// <summary>
    /// The canonical kick sequence.
    /// Tries no kick first, then small translations.
    /// </summary>
    public static readonly IReadOnlyList<Int3> Kicks =
    [
        new(0, 0, 0),
        new(1, 0, 0), new(-1, 0, 0),
        new(0, 0, 1), new(0, 0, -1),
        new(0, 1, 0),
        new(1, 0, 1), new(1, 0, -1), new(-1, 0, 1), new(-1, 0, -1)
    ];
}
