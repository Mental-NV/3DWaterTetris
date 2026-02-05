using System;
using System.Collections.Generic;
using Floodline.Core;
using Floodline.Core.Levels;
using Floodline.Core.Movement;
using Floodline.Core.Random;
using Xunit;

namespace Floodline.Core.Tests.Golden;

public class DrainPlacementGoldenTests
{
    private static readonly Int3[] HorizontalAdjOffsets =
    [
        new Int3(1, 0, 0),
        new Int3(-1, 0, 0),
        new Int3(0, 0, 1),
        new Int3(0, 0, -1)
    ];

    [Fact]
    public void Golden_DrainPlacement_Pivot_Displacement()
    {
        AbilitiesConfig abilities = new(
            DrainPlacementCharges: 1,
            DrainPlacement: new DrainConfig(1, DrainScope.Self));

        Simulation sim = new(CreateLevel(abilities), new Pcg32(30));
        ActivePiece piece = sim.ActivePiece!;
        Int3 landingOrigin = GetLandingOrigin(piece);

        sim.Grid.SetVoxel(new Int3(0, 0, 0), Voxel.Water);
        sim.Grid.SetVoxel(landingOrigin, Voxel.Water);

        sim.Tick(InputCommand.DrainPlacementAbility);
        sim.Tick(InputCommand.HardDrop);

        string snapshot = AbilitySnapshotWriter.Write("drain_pivot_displacement", sim);
        GoldenAssert.Matches("abilities/drain_pivot_displacement", snapshot);
    }

    [Fact]
    public void Golden_DrainPlacement_Removes_Adjacent_Water()
    {
        AbilitiesConfig abilities = new(
            DrainPlacementCharges: 1,
            DrainPlacement: new DrainConfig(1, DrainScope.Adj6));

        Simulation sim = new(CreateLevel(abilities), new Pcg32(31));
        ActivePiece piece = sim.ActivePiece!;
        Int3 landingOrigin = GetLandingOrigin(piece);
        Int3 offset = FindHorizontalOffset(piece);
        Int3 waterPos = landingOrigin + offset;

        PlaceHorizontalRing(waterPos, landingOrigin, sim.Grid);
        sim.Grid.SetVoxel(waterPos, Voxel.Water);

        sim.Tick(InputCommand.DrainPlacementAbility);
        sim.Tick(InputCommand.HardDrop);

        string snapshot = AbilitySnapshotWriter.Write("drain_adjacent_remove", sim);
        GoldenAssert.Matches("abilities/drain_adjacent_remove", snapshot);
    }

    [Fact]
    public void Golden_DrainPlacement_NoCharges_NoDrain()
    {
        AbilitiesConfig abilities = new(
            DrainPlacementCharges: 0,
            DrainPlacement: new DrainConfig(1, DrainScope.Adj6));

        Simulation sim = new(CreateLevel(abilities), new Pcg32(32));

        sim.Tick(InputCommand.DrainPlacementAbility);
        sim.Tick(InputCommand.HardDrop);

        string snapshot = AbilitySnapshotWriter.Write("drain_no_charges", sim);
        GoldenAssert.Matches("abilities/drain_no_charges", snapshot);
    }

    private static Level CreateLevel(AbilitiesConfig abilities) =>
        new(
            new LevelMeta("drain-golden", "Drain Golden", "0.2.2", 12345U),
            new Int3(10, 20, 10),
            [],
            [],
            new RotationConfig(),
            new BagConfig("FIXED_SEQUENCE", ["O2:STANDARD"], null),
            [],
            abilities);

    private static Int3 GetLandingOrigin(ActivePiece piece)
    {
        int minYOffset = GetMinOffsetY(piece);
        return new Int3(piece.Origin.X, -minYOffset, piece.Origin.Z);
    }

    private static int GetMinOffsetY(ActivePiece piece)
    {
        int min = int.MaxValue;
        foreach (Int3 offset in piece.Piece.Voxels)
        {
            if (offset.Y < min)
            {
                min = offset.Y;
            }
        }

        return min;
    }

    private static Int3 FindHorizontalOffset(ActivePiece piece)
    {
        HashSet<Int3> occupiedOffsets = [.. piece.Piece.Voxels];
        foreach (Int3 offset in HorizontalAdjOffsets)
        {
            if (!occupiedOffsets.Contains(offset))
            {
                return offset;
            }
        }

        throw new InvalidOperationException("No horizontal offset available for drain test.");
    }

    private static void PlaceHorizontalRing(Int3 waterPos, Int3 pivot, Grid grid)
    {
        Int3 toPivot = pivot - waterPos;
        foreach (Int3 offset in HorizontalAdjOffsets)
        {
            if (offset == toPivot)
            {
                continue;
            }

            Int3 blocker = waterPos + offset;
            if (grid.IsInBounds(blocker))
            {
                grid.SetVoxel(blocker, new Voxel(OccupancyType.Bedrock));
            }
        }
    }
}
