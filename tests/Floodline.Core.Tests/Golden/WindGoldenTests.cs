using System;
using System.Collections.Generic;
using System.Text;
using Floodline.Core;
using Floodline.Core.Levels;
using Floodline.Core.Movement;
using Floodline.Core.Random;
using Xunit;

namespace Floodline.Core.Tests.Golden;

public class WindGoldenTests
{
    private const string SnapshotVersion = "0.1";
    private const char LineBreak = '\n';

    [Fact]
    public void Golden_Wind_Schedule_Alternate_Direction()
    {
        WindSpec spec = new(IntervalTicks: 2, OffsetTicks: 1, PushStrength: 1, DirectionMode: "ALTERNATE_EW", FixedDirection: null);
        HazardConfig hazard = CreateWindHazard(spec);
        Simulation sim = new(CreateLevel(hazard, "O2:STANDARD"), new Pcg32(11));

        string snapshot = WriteTimelineSnapshot("schedule_alternate", spec, sim, ticks: 4);

        GoldenAssert.Matches("wind/schedule_alternate", snapshot);
    }

    [Fact]
    public void Golden_Wind_PushStrength_And_Heavy_Reduction()
    {
        WindSpec spec = new(IntervalTicks: 1, OffsetTicks: 0, PushStrength: 2, DirectionMode: "FIXED", FixedDirection: "EAST");
        HazardConfig hazard = CreateWindHazard(spec);

        Simulation standard = new(CreateLevel(hazard, "O2:STANDARD"), new Pcg32(12));
        Simulation heavy = new(CreateLevel(hazard, "O2:HEAVY"), new Pcg32(12));

        StringBuilder builder = new();
        AppendLine(builder, $"# FloodlineWindSnapshot v{SnapshotVersion}");
        AppendLine(builder, "scenario: push_strength_vs_heavy");
        AppendLine(builder, $"wind: {FormatSpec(spec)}");

        AppendSection(builder, "standard", standard, ticks: 1);
        AppendSection(builder, "heavy", heavy, ticks: 1);

        GoldenAssert.Matches("wind/push_strength_heavy", builder.ToString());
    }

    [Fact]
    public void Golden_Wind_Collision_Stops_Further_Push()
    {
        WindSpec spec = new(IntervalTicks: 1, OffsetTicks: 0, PushStrength: 2, DirectionMode: "FIXED", FixedDirection: "EAST");
        HazardConfig hazard = CreateWindHazard(spec);

        Simulation sim = new(CreateLevel(hazard, "O2:STANDARD"), new Pcg32(13));
        Int3 blocker = PlaceSecondStepBlocker(sim, new Int3(1, 0, 0));

        string snapshot = WriteTimelineSnapshot("collision_stop", spec, sim, ticks: 1, blocker);

        GoldenAssert.Matches("wind/collision_stop", snapshot);
    }

    private static Level CreateLevel(HazardConfig hazard, string bagToken) =>
        new(
            new LevelMeta("wind-golden", "Wind Golden", "0.2.0", 777U),
            new Int3(12, 10, 12),
            [],
            [],
            new RotationConfig(),
            new BagConfig("FIXED_SEQUENCE", [bagToken], null),
            [hazard]
        );

    private static HazardConfig CreateWindHazard(WindSpec spec)
    {
        Dictionary<string, object> parameters = new()
        {
            ["intervalTicks"] = spec.IntervalTicks,
            ["pushStrength"] = spec.PushStrength,
            ["directionMode"] = spec.DirectionMode,
            ["firstGustOffsetTicks"] = spec.OffsetTicks
        };

        if (!string.IsNullOrWhiteSpace(spec.FixedDirection))
        {
            parameters["fixedDirection"] = spec.FixedDirection;
        }

        return new HazardConfig("WIND_GUST", true, parameters);
    }

    private static string WriteTimelineSnapshot(string scenario, WindSpec spec, Simulation sim, int ticks, Int3? blocker = null)
    {
        StringBuilder builder = new();
        AppendLine(builder, $"# FloodlineWindSnapshot v{SnapshotVersion}");
        AppendLine(builder, $"scenario: {scenario}");
        AppendLine(builder, $"wind: {FormatSpec(spec)}");
        AppendLine(builder, $"bounds: {FormatBounds(sim.Grid.Size)}");
        AppendLine(builder, $"gravity: {sim.Gravity}");
        AppendLine(builder, $"piece: {FormatPiece(sim.ActivePiece)}");
        AppendLine(builder, $"start: origin={FormatOrigin(sim.ActivePiece)}");

        if (blocker.HasValue)
        {
            AppendLine(builder, $"blocker: {FormatInt3(blocker.Value)}");
        }

        AppendLine(builder, $"ticks: {ticks}");

        for (int i = 0; i < ticks; i++)
        {
            sim.Tick(InputCommand.None);
            AppendLine(builder, $"tick: {sim.State.TicksElapsed} origin={FormatOrigin(sim.ActivePiece)}");
        }

        return builder.ToString();
    }

    private static void AppendSection(StringBuilder builder, string label, Simulation sim, int ticks)
    {
        AppendLine(builder, $"section: {label}");
        AppendLine(builder, $"bounds: {FormatBounds(sim.Grid.Size)}");
        AppendLine(builder, $"gravity: {sim.Gravity}");
        AppendLine(builder, $"piece: {FormatPiece(sim.ActivePiece)}");
        AppendLine(builder, $"start: origin={FormatOrigin(sim.ActivePiece)}");
        AppendLine(builder, $"ticks: {ticks}");

        for (int i = 0; i < ticks; i++)
        {
            sim.Tick(InputCommand.None);
            AppendLine(builder, $"tick: {sim.State.TicksElapsed} origin={FormatOrigin(sim.ActivePiece)}");
        }
    }

    private static Int3 PlaceSecondStepBlocker(Simulation sim, Int3 direction)
    {
        ActivePiece piece = sim.ActivePiece ?? throw new InvalidOperationException("Active piece required for wind blocker.");
        HashSet<Int3> step1 = [];
        foreach (Int3 pos in piece.GetWorldPositions())
        {
            step1.Add(pos + direction);
        }

        foreach (Int3 pos in piece.GetWorldPositions())
        {
            Int3 candidate = pos + direction + direction;
            if (!sim.Grid.IsInBounds(candidate))
            {
                continue;
            }

            if (step1.Contains(candidate))
            {
                continue;
            }

            sim.Grid.SetVoxel(candidate, new Voxel(OccupancyType.Solid, "STANDARD"));
            return candidate;
        }

        throw new InvalidOperationException("No valid blocker location found for wind collision test.");
    }

    private static string FormatSpec(WindSpec spec)
    {
        string fixedDirection = string.IsNullOrWhiteSpace(spec.FixedDirection)
            ? "none"
            : spec.FixedDirection.Trim().ToUpperInvariant();

        return $"interval={spec.IntervalTicks} offset={spec.OffsetTicks} pushStrength={spec.PushStrength} directionMode={spec.DirectionMode} fixedDirection={fixedDirection}";
    }

    private static string FormatBounds(Int3 bounds) => $"{bounds.X} {bounds.Y} {bounds.Z}";

    private static string FormatPiece(ActivePiece? piece)
    {
        if (piece is null)
        {
            return "none";
        }

        return $"id={piece.Piece.Id} material={FormatMaterial(piece.MaterialId)}";
    }

    private static string FormatOrigin(ActivePiece? piece) =>
        piece is null ? "none" : FormatInt3(piece.Origin);

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

    private readonly record struct WindSpec(
        int IntervalTicks,
        int OffsetTicks,
        int PushStrength,
        string DirectionMode,
        string? FixedDirection);
}
