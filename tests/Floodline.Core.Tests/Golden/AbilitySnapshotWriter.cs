using System.Collections.Generic;
using System.Text;
using Floodline.Core;
using Floodline.Core.Movement;

namespace Floodline.Core.Tests.Golden;

internal static class AbilitySnapshotWriter
{
    private const string SnapshotVersion = "0.1";
    private const char LineBreak = '\n';

    public static string Write(string scenario, Simulation sim)
    {
        StringBuilder builder = new();
        AppendHeader(builder, scenario);
        AppendSection(builder, "state", sim);
        return builder.ToString();
    }

    public static void AppendHeader(StringBuilder builder, string scenario)
    {
        AppendLine(builder, $"# FloodlineAbilitySnapshot v{SnapshotVersion}");
        AppendLine(builder, $"scenario: {scenario}");
    }

    public static void AppendSection(StringBuilder builder, string label, Simulation sim)
    {
        AppendLine(builder, $"section: {label}");
        AppendLine(builder, $"ticks: {sim.State.TicksElapsed}");
        AppendLine(builder, $"status: {sim.State.Status}");
        AppendLine(builder, $"gravity: {sim.Gravity}");
        AppendLine(builder, $"activePiece: {FormatActivePiece(sim.ActivePiece)}");

        List<CellSnapshot> cells = CollectCells(sim.Grid);
        AppendLine(builder, $"cells: {cells.Count}");
        foreach (CellSnapshot cell in cells)
        {
            AppendLine(builder, $"cell: {FormatInt3(cell.Position)} type={cell.Type} material={cell.Material}");
        }
    }

    private static List<CellSnapshot> CollectCells(Grid grid)
    {
        List<CellSnapshot> cells = [];
        for (int x = 0; x < grid.Size.X; x++)
        {
            for (int y = 0; y < grid.Size.Y; y++)
            {
                for (int z = 0; z < grid.Size.Z; z++)
                {
                    Int3 pos = new(x, y, z);
                    Voxel voxel = grid.GetVoxel(pos);
                    if (voxel.Type == OccupancyType.Empty)
                    {
                        continue;
                    }

                    cells.Add(new CellSnapshot(pos, voxel.Type, FormatMaterial(voxel.MaterialId)));
                }
            }
        }

        return cells;
    }

    private static string FormatActivePiece(ActivePiece? piece)
    {
        if (piece is null)
        {
            return "none";
        }

        OrientedPiece oriented = piece.Piece;
        return $"id={oriented.Id} orientation={oriented.OrientationIndex} origin={FormatInt3(piece.Origin)}";
    }

    private static string FormatMaterial(string? materialId)
    {
        if (string.IsNullOrWhiteSpace(materialId))
        {
            return "none";
        }

        return materialId.Trim().ToUpperInvariant();
    }

    private static string FormatInt3(Int3 pos) => $"{pos.X},{pos.Y},{pos.Z}";

    private static void AppendLine(StringBuilder builder, string line) => builder.Append(line).Append(LineBreak);

    private readonly record struct CellSnapshot(Int3 Position, OccupancyType Type, string Material);
}
