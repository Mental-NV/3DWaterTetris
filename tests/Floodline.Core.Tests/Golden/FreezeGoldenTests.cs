using System;
using System.Collections.Generic;
using System.Text;
using Floodline.Core;
using Floodline.Core.Levels;
using Floodline.Core.Movement;
using Floodline.Core.Random;
using Xunit;

namespace Floodline.Core.Tests.Golden;

public class FreezeGoldenTests
{
    private static readonly Int3[] Adj6Offsets =
    [
        new Int3(1, 0, 0),
        new Int3(-1, 0, 0),
        new Int3(0, 1, 0),
        new Int3(0, -1, 0),
        new Int3(0, 0, 1),
        new Int3(0, 0, -1)
    ];

    private static readonly Int3[] DiagonalOffsets =
    [
        new Int3(1, 1, 1),
        new Int3(1, 1, -1),
        new Int3(1, -1, 1),
        new Int3(1, -1, -1),
        new Int3(-1, 1, 1),
        new Int3(-1, 1, -1),
        new Int3(-1, -1, 1),
        new Int3(-1, -1, -1)
    ];

    [Fact]
    public void Golden_Freeze_Adj6_Targeting()
    {
        Level level = CreateLevel(new AbilitiesConfig(
            FreezeCharges: 1,
            FreezeDurationResolves: 2,
            FreezeScope: FreezeScope.Adj6));

        Simulation sim = new(level, new Pcg32(20));
        ActivePiece piece = sim.ActivePiece!;

        Int3 adjacent = FindLandingOffsetCell(piece, sim.Grid, Adj6Offsets);
        Int3 diagonal = FindLandingOffsetCell(piece, sim.Grid, DiagonalOffsets);

        sim.Grid.SetVoxel(adjacent, Voxel.Water);
        sim.Grid.SetVoxel(diagonal, Voxel.Water);

        sim.Tick(InputCommand.FreezeAbility);
        sim.Tick(InputCommand.HardDrop);

        string snapshot = AbilitySnapshotWriter.Write("freeze_adj6_targeting", sim);
        GoldenAssert.Matches("abilities/freeze_adj6_targeting", snapshot);
    }

    [Fact]
    public void Golden_Freeze_Adj26_Thaw_Reflow()
    {
        Level level = CreateLevel(new AbilitiesConfig(
            FreezeCharges: 1,
            FreezeDurationResolves: 2,
            FreezeScope: FreezeScope.Adj26));

        Simulation sim = new(level, new Pcg32(21));
        ActivePiece piece = sim.ActivePiece!;
        Int3 target = FindDiagonalAboveEmpty(piece, sim.Grid);

        sim.Grid.SetVoxel(target, Voxel.Water);

        StringBuilder builder = new();
        AbilitySnapshotWriter.AppendHeader(builder, "freeze_adj26_thaw_reflow");

        sim.Tick(InputCommand.FreezeAbility);
        sim.Tick(InputCommand.HardDrop);
        AbilitySnapshotWriter.AppendSection(builder, "after_freeze", sim);

        sim.Tick(InputCommand.HardDrop);
        AbilitySnapshotWriter.AppendSection(builder, "after_thaw", sim);

        GoldenAssert.Matches("abilities/freeze_adj26_thaw_reflow", builder.ToString());
    }

    private static Level CreateLevel(AbilitiesConfig abilities) =>
        new(
            new LevelMeta("freeze-golden", "Freeze Golden", "0.2.2", 12345U),
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

    private static Int3 FindLandingOffsetCell(ActivePiece piece, Grid grid, IReadOnlyList<Int3> offsets)
    {
        HashSet<Int3> occupiedOffsets = [.. piece.Piece.Voxels];
        Int3 landingOrigin = GetLandingOrigin(piece);
        foreach (Int3 voxelOffset in occupiedOffsets)
        {
            Int3 pos = landingOrigin + voxelOffset;
            foreach (Int3 offset in offsets)
            {
                Int3 candidate = pos + offset;
                if (grid.IsInBounds(candidate) && !occupiedOffsets.Contains(candidate - landingOrigin))
                {
                    return candidate;
                }
            }
        }

        throw new InvalidOperationException("No candidate offset cell found for freeze test.");
    }

    private static Int3 FindDiagonalAboveEmpty(ActivePiece piece, Grid grid)
    {
        HashSet<Int3> occupiedOffsets = [.. piece.Piece.Voxels];
        Int3 landingOrigin = GetLandingOrigin(piece);

        foreach (Int3 voxelOffset in occupiedOffsets)
        {
            Int3 pos = landingOrigin + voxelOffset;
            foreach (Int3 offset in DiagonalOffsets)
            {
                if (offset.Y <= 0)
                {
                    continue;
                }

                Int3 candidate = pos + offset;
                if (!grid.IsInBounds(candidate))
                {
                    continue;
                }

                if (occupiedOffsets.Contains(candidate - landingOrigin))
                {
                    continue;
                }

                Int3 below = candidate + new Int3(0, -1, 0);
                if (!grid.IsInBounds(below))
                {
                    continue;
                }

                if (occupiedOffsets.Contains(below - landingOrigin))
                {
                    continue;
                }

                if (grid.GetVoxel(below).Type != OccupancyType.Empty)
                {
                    continue;
                }

                return candidate;
            }
        }

        throw new InvalidOperationException("No diagonal-above-empty target found for freeze test.");
    }
}
