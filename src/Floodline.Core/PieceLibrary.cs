using System.Collections.ObjectModel;

namespace Floodline.Core;

/// <summary>
/// Central registry for all piece definitions in Level Content Pack v0.2.
/// </summary>
public static class PieceLibrary
{
    private static readonly ReadOnlyDictionary<PieceId, PieceDefinition> PiecesMap;

    static PieceLibrary()
    {
        List<PieceDefinition> definitions =
        [
            CreateDefinition(PieceId.O2, [new(0, 0, 0), new(1, 0, 0), new(0, 0, 1), new(1, 0, 1)], ["flat", "tutorial"]),
            CreateDefinition(PieceId.I3, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0)], ["tutorial"]),
            CreateDefinition(PieceId.I4, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(3, 0, 0)], []),
            CreateDefinition(PieceId.L3, [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)], ["tutorial"]),
            CreateDefinition(PieceId.L4, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(0, 1, 0)], []),
            CreateDefinition(PieceId.J4, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(2, 1, 0)], []),
            CreateDefinition(PieceId.T3, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(1, 1, 0)], []),
            CreateDefinition(PieceId.S4, [new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(2, 0, 1)], []),
            CreateDefinition(PieceId.Z4, [new(0, 0, 0), new(1, 0, 0), new(1, 0, -1), new(2, 0, -1)], []),
            CreateDefinition(PieceId.U5, [new(0, 0, 0), new(2, 0, 0), new(0, 0, 1), new(1, 0, 1), new(2, 0, 1)], []),
            CreateDefinition(PieceId.P5, [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0), new(2, 0, 0)], []),
            CreateDefinition(PieceId.C3D5, [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 0, 1)], ["3d"])
        ];

        Dictionary<PieceId, PieceDefinition> dict = [];
        foreach (PieceDefinition def in definitions)
        {
            dict.Add(def.Id, def);
        }

        PiecesMap = new ReadOnlyDictionary<PieceId, PieceDefinition>(dict);
    }

    private static PieceDefinition CreateDefinition(PieceId id, Int3[] voxels, string[] tags)
    {
        (IReadOnlyList<IReadOnlyList<Int3>> rotated, IReadOnlyList<IReadOnlyList<Int3>> normalized) = OrientationGenerator.GetUniqueOrientations(voxels);
        return new PieceDefinition(id, voxels, tags)
        {
            UniqueOrientations = rotated,
            NormalizedOrientations = normalized
        };
    }

    public static PieceDefinition Get(PieceId id) => PiecesMap.TryGetValue(id, out PieceDefinition? definition)
            ? definition
            : throw new KeyNotFoundException($"Piece definition for {id} not found.");

    public static IEnumerable<PieceDefinition> All() => PiecesMap.Values;

    /// <summary>
    /// Returns the expected oriented piece structure after applying a rotation matrix.
    /// Performs a lookup in the piece's canonical orientation list to maintain valid orientation indices.
    /// </summary>
    /// <param name="piece">The current oriented piece.</param>
    /// <param name="rotation">The rotation matrix to apply.</param>
    /// <returns>A new oriented piece representing the rotated state.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resulting orientation is not found in the library.</exception>
    public static OrientedPiece Rotate(OrientedPiece piece, Matrix3x3 rotation)
    {
        PieceDefinition def = Get(piece.Id);
        List<Int3> rotated = [.. piece.Voxels.Select(rotation.Transform)];
        List<Int3> normalized = OrientationGenerator.Normalize(rotated);

        // Find matching orientation index using NormalizedOrientations
        for (int i = 0; i < def.NormalizedOrientations.Count; i++)
        {
            if (AreVoxelsEqual(def.NormalizedOrientations[i], normalized))
            {
                return new OrientedPiece(piece.Id, def.UniqueOrientations[i], i);
            }
        }

        // Should not happen if AllRotations covers everything and UniqueOrientations is complete
        throw new InvalidOperationException($"Could not find matching orientation for rotated piece {piece.Id}");
    }

    private static bool AreVoxelsEqual(IReadOnlyList<Int3> a, List<Int3> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }
        // Voxels are sorted by Normalize, so we can iterate in order
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }
        return true;
    }
}
