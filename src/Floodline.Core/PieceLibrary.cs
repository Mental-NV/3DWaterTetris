using System.Collections.ObjectModel;

namespace Floodline.Core;

public static class PieceLibrary
{
    private static readonly ReadOnlyDictionary<PieceId, PieceDefinition> PiecesMap;

    static PieceLibrary()
    {
        List<PieceDefinition> definitions =
        [
            new(PieceId.O2, [new(0, 0, 0), new(1, 0, 0), new(0, 0, 1), new(1, 0, 1)], ["flat", "tutorial"]),
            new(PieceId.I3, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0)], ["tutorial"]),
            new(PieceId.I4, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(3, 0, 0)], []),
            new(PieceId.L3, [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)], ["tutorial"]),
            new(PieceId.L4, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(0, 1, 0)], []),
            new(PieceId.J4, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(2, 1, 0)], []),
            new(PieceId.T3, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(1, 1, 0)], []),
            new(PieceId.S4, [new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(2, 0, 1)], []),
            new(PieceId.Z4, [new(0, 0, 0), new(1, 0, 0), new(1, 0, -1), new(2, 0, -1)], []),
            new(PieceId.U5, [new(0, 0, 0), new(2, 0, 0), new(0, 0, 1), new(1, 0, 1), new(2, 0, 1)], []),
            new(PieceId.P5, [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0), new(2, 0, 0)], []),
            new(PieceId.C3D5, [new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 0, 1)], ["3d"])
        ];

        Dictionary<PieceId, PieceDefinition> dict = [];
        foreach (PieceDefinition def in definitions)
        {
            dict.Add(def.Id, def);
        }

        PiecesMap = new ReadOnlyDictionary<PieceId, PieceDefinition>(dict);
    }

    public static PieceDefinition Get(PieceId id) => PiecesMap.TryGetValue(id, out PieceDefinition? definition)
            ? definition
            : throw new KeyNotFoundException($"Piece definition for {id} not found.");

    public static IEnumerable<PieceDefinition> All() => PiecesMap.Values;
}
