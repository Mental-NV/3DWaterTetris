using System;
using System.Reflection;
using System.Text;
using Floodline.Core;
using Floodline.Core.Levels;
using Floodline.Core.Movement;
using Floodline.Core.Random;
using Xunit;

namespace Floodline.Core.Tests.Golden;

public class RotationConstraintGoldenTests
{
    private const string SnapshotVersion = "0.1";
    private const char LineBreak = '\n';

    private static readonly PropertyInfo RotationsExecutedProperty =
        typeof(Simulation).GetProperty("RotationsExecuted", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("RotationsExecuted property not found on Simulation.");

    private static readonly PropertyInfo RotationCooldownProperty =
        typeof(Simulation).GetProperty("RotationCooldownRemaining", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("RotationCooldownRemaining property not found on Simulation.");

    [Fact]
    public void Golden_Rotation_AllowedDirections_Accept()
    {
        RotationConfig rotation = new(AllowedDirections: ["NORTH"]);
        Simulation sim = new(CreateLevel(rotation), new Pcg32(1));

        sim.Tick(InputCommand.RotateWorldForward);

        GoldenAssert.Matches("rotation/allowed_directions_accept", WriteRotationSnapshot(sim));
    }

    [Fact]
    public void Golden_Rotation_AllowedDirections_Reject()
    {
        RotationConfig rotation = new(AllowedDirections: ["DOWN"]);
        Simulation sim = new(CreateLevel(rotation), new Pcg32(2));

        sim.Tick(InputCommand.RotateWorldForward);

        GoldenAssert.Matches("rotation/allowed_directions_reject", WriteRotationSnapshot(sim));
    }

    [Fact]
    public void Golden_Rotation_Cooldown_Rejects_During_Cooldown()
    {
        RotationConfig rotation = new(CooldownTicks: 2);
        Simulation sim = new(CreateLevel(rotation), new Pcg32(3));

        sim.Tick(InputCommand.RotateWorldForward);
        sim.Tick(InputCommand.RotateWorldBack);

        GoldenAssert.Matches("rotation/cooldown_reject", WriteRotationSnapshot(sim));
    }

    [Fact]
    public void Golden_Rotation_Cooldown_Allows_After_Expiry()
    {
        RotationConfig rotation = new(CooldownTicks: 2);
        Simulation sim = new(CreateLevel(rotation), new Pcg32(4));

        sim.Tick(InputCommand.RotateWorldForward);
        sim.Tick(InputCommand.RotateWorldBack);
        sim.Tick(InputCommand.None);
        sim.Tick(InputCommand.RotateWorldBack);

        GoldenAssert.Matches("rotation/cooldown_accept_after", WriteRotationSnapshot(sim));
    }

    [Fact]
    public void Golden_Rotation_MaxRotations_Rejects_After_Limit()
    {
        RotationConfig rotation = new(MaxRotations: 1);
        Simulation sim = new(CreateLevel(rotation), new Pcg32(5));

        sim.Tick(InputCommand.RotateWorldForward);
        sim.Tick(InputCommand.RotateWorldBack);

        GoldenAssert.Matches("rotation/max_rotations_reject", WriteRotationSnapshot(sim));
    }

    [Fact]
    public void Golden_Rotation_TiltBudget_Exhausted()
    {
        RotationConfig rotation = new(TiltBudget: 1);
        Simulation sim = new(CreateLevel(rotation), new Pcg32(6));

        sim.Tick(InputCommand.RotateWorldForward);
        sim.Tick(InputCommand.RotateWorldBack);

        GoldenAssert.Matches("rotation/tilt_budget_exhausted", WriteRotationSnapshot(sim));
    }

    private static Level CreateLevel(RotationConfig rotation) =>
        new(
            new("rotation-golden", "Rotation Golden", "0.2.0", 1U),
            new(10, 10, 10),
            [],
            [],
            rotation,
            new("FIXED_SEQUENCE", ["I4"], null),
            []
        );

    private static string WriteRotationSnapshot(Simulation sim)
    {
        StringBuilder builder = new();
        AppendLine(builder, $"# FloodlineRotationSnapshot v{SnapshotVersion}");
        AppendLine(builder, $"gravity: {sim.Gravity}");
        AppendLine(builder, $"ticks: {sim.State.TicksElapsed}");
        AppendLine(builder, $"piecesLocked: {sim.State.PiecesLocked}");
        AppendLine(builder, $"rotationsExecuted: {GetRotationsExecuted(sim)}");
        AppendLine(builder, $"rotationCooldownRemaining: {GetRotationCooldownRemaining(sim)}");

        if (sim.ActivePiece is null)
        {
            AppendLine(builder, "activePiece: none");
        }
        else
        {
            OrientedPiece piece = sim.ActivePiece.Piece;
            AppendLine(
                builder,
                $"activePiece: id={piece.Id} orientation={piece.OrientationIndex} origin={FormatInt3(sim.ActivePiece.Origin)}");
        }

        return builder.ToString();
    }

    private static int GetRotationsExecuted(Simulation sim) =>
        (int)(RotationsExecutedProperty.GetValue(sim) ?? 0);

    private static int GetRotationCooldownRemaining(Simulation sim) =>
        (int)(RotationCooldownProperty.GetValue(sim) ?? 0);

    private static string FormatInt3(Int3 pos) => $"{pos.X},{pos.Y},{pos.Z}";

    private static void AppendLine(StringBuilder builder, string line) => builder.Append(line).Append(LineBreak);
}
