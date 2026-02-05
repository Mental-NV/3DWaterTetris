using System.Collections.Generic;
using System.Text;
using Floodline.Core;
using Floodline.Core.Levels;
using Floodline.Core.Movement;
using Floodline.Core.Random;
using Xunit;

namespace Floodline.Core.Tests.Golden;

public class StabilizeGoldenTests
{
    private const string SnapshotVersion = "0.1";
    private const char LineBreak = '\n';

    [Fact]
    public void Golden_Stabilize_Anchors_Expire()
    {
        Simulation sim = new(CreateLevel(stabilizeCharges: 1), new Pcg32(21));

        StringBuilder builder = new();
        AppendLine(builder, $"# FloodlineStabilizeSnapshot v{SnapshotVersion}");
        AppendLine(builder, "scenario: anchor_expiry");

        sim.Tick(InputCommand.Stabilize);
        sim.Tick(InputCommand.RotatePieceYawCW);
        sim.Tick(InputCommand.HardDrop);

        AppendSection(builder, "after_lock", sim);

        sim.Tick(InputCommand.RotateWorldLeft);
        AppendSection(builder, "after_rotation_1", sim);

        sim.Tick(InputCommand.RotateWorldForward);
        AppendSection(builder, "after_rotation_2", sim);

        sim.Tick(InputCommand.RotateWorldForward);
        AppendSection(builder, "after_rotation_3", sim);

        GoldenAssert.Matches("stabilize/anchor_expiry", builder.ToString());
    }

    [Fact]
    public void Golden_Stabilize_Disabled_NoAnchors()
    {
        Simulation sim = new(CreateLevel(stabilizeCharges: 0), new Pcg32(22));

        StringBuilder builder = new();
        AppendLine(builder, $"# FloodlineStabilizeSnapshot v{SnapshotVersion}");
        AppendLine(builder, "scenario: disabled_no_anchors");

        sim.Tick(InputCommand.Stabilize);
        sim.Tick(InputCommand.HardDrop);

        AppendSection(builder, "after_lock", sim);

        GoldenAssert.Matches("stabilize/disabled_no_anchors", builder.ToString());
    }

    private static Level CreateLevel(int stabilizeCharges)
    {
        AbilitiesConfig abilities = new(StabilizeCharges: stabilizeCharges);

        return new Level(
            new LevelMeta("stabilize-golden", "Stabilize Golden", "0.2.0", 900U),
            new Int3(10, 10, 10),
            [],
            [],
            new RotationConfig(),
            new BagConfig("FIXED_SEQUENCE", ["I4:STANDARD"], null),
            [],
            abilities
        );
    }

    private static void AppendSection(StringBuilder builder, string label, Simulation sim)
    {
        AppendLine(builder, $"section: {label}");
        AppendLine(builder, $"ticks: {sim.State.TicksElapsed}");
        AppendLine(builder, $"gravity: {sim.Gravity}");
        AppendLine(builder, $"anchoredSolids: {CountAnchoredSolids(sim.Grid)}");
        AppendLine(builder, $"activePiece: {FormatActivePiece(sim.ActivePiece)}");

        List<CellSnapshot> cells = CollectCells(sim.Grid);
        AppendLine(builder, $"cells: {cells.Count}");
        foreach (CellSnapshot cell in cells)
        {
            AppendLine(builder, $"cell: {FormatInt3(cell.Position)} type={cell.Type} material={cell.Material} anchored={FormatBool(cell.Anchored)}");
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

                    cells.Add(new CellSnapshot(pos, voxel.Type, FormatMaterial(voxel.MaterialId), voxel.Anchored));
                }
            }
        }

        return cells;
    }

    private static int CountAnchoredSolids(Grid grid)
    {
        int count = 0;
        for (int x = 0; x < grid.Size.X; x++)
        {
            for (int y = 0; y < grid.Size.Y; y++)
            {
                for (int z = 0; z < grid.Size.Z; z++)
                {
                    Voxel voxel = grid.GetVoxel(new Int3(x, y, z));
                    if (voxel.Type == OccupancyType.Solid && voxel.Anchored)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
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

    private static string FormatBool(bool value) => value ? "true" : "false";

    private static string FormatInt3(Int3 pos) => $"{pos.X},{pos.Y},{pos.Z}";

    private static void AppendLine(StringBuilder builder, string line) => builder.Append(line).Append(LineBreak);

    private readonly record struct CellSnapshot(Int3 Position, OccupancyType Type, string Material, bool Anchored);
}
