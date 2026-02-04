using Floodline.Core;
using Floodline.Core.Levels;
using Floodline.Core.Movement;
using Floodline.Core.Random;
using Xunit;

namespace Floodline.Core.Tests.Golden;

public class LockDelayGoldenTests
{
    [Fact]
    public void Golden_LockDelay_Countdown_BeforeLock()
    {
        Simulation sim = CreateSimulationWithGroundPlane();
        int delay = Constants.LockDelayTicks;

        TickNTimes(sim, InputCommand.None, delay - 1);

        GoldenAssert.Matches(
            "lock_delay/before_lock",
            SnapshotWriter.Write(sim.Grid, sim.Gravity, sim.State, activePiece: sim.ActivePiece));
    }

    [Fact]
    public void Golden_LockDelay_Countdown_AfterLock()
    {
        Simulation sim = CreateSimulationWithGroundPlane();
        int delay = Constants.LockDelayTicks;

        TickNTimes(sim, InputCommand.None, delay);

        GoldenAssert.Matches(
            "lock_delay/after_lock",
            SnapshotWriter.Write(sim.Grid, sim.Gravity, sim.State, activePiece: sim.ActivePiece));
    }

    [Fact]
    public void Golden_LockDelay_Reset_On_Ungrounded()
    {
        Simulation sim = CreateSimulationWithGroundPlane();
        int delay = Constants.LockDelayTicks;

        TickNTimes(sim, InputCommand.None, delay - 1);

        sim.Tick(InputCommand.RotateWorldForward);
        sim.Tick(InputCommand.RotateWorldBack);

        TickNTimes(sim, InputCommand.None, delay - 2);

        GoldenAssert.Matches(
            "lock_delay/reset_on_unground",
            SnapshotWriter.Write(sim.Grid, sim.Gravity, sim.State, activePiece: sim.ActivePiece));
    }

    [Fact]
    public void Golden_LockDelay_Reset_Cap_Enforced()
    {
        Simulation sim = CreateSimulationWithGroundPlane();
        int delay = Constants.LockDelayTicks;
        int maxResets = Constants.MaxLockResets;

        TickNTimes(sim, InputCommand.None, delay - 1);

        for (int i = 0; i < maxResets; i++)
        {
            sim.Tick(InputCommand.RotateWorldForward);
            sim.Tick(InputCommand.RotateWorldBack);

            if (i < maxResets - 1)
            {
                TickNTimes(sim, InputCommand.None, delay - 2);
            }
        }

        TickNTimes(sim, InputCommand.None, delay - 2);

        sim.Tick(InputCommand.RotateWorldForward);
        sim.Tick(InputCommand.RotateWorldBack);

        GoldenAssert.Matches(
            "lock_delay/reset_cap",
            SnapshotWriter.Write(sim.Grid, sim.Gravity, sim.State, activePiece: sim.ActivePiece));
    }

    [Fact]
    public void Golden_HardDrop_Locks_Immediately()
    {
        Simulation sim = new(CreateTestLevel(), new Pcg32(1));

        sim.Tick(InputCommand.HardDrop);

        GoldenAssert.Matches(
            "lock_delay/hard_drop",
            SnapshotWriter.Write(sim.Grid, sim.Gravity, sim.State, activePiece: sim.ActivePiece));
    }

    [Fact]
    public void Golden_SoftDrop_DoesNot_Lock_Immediately()
    {
        Simulation sim = new(CreateTestLevel(), new Pcg32(2));

        sim.Tick(InputCommand.SoftDrop);

        GoldenAssert.Matches(
            "lock_delay/soft_drop",
            SnapshotWriter.Write(sim.Grid, sim.Gravity, sim.State, activePiece: sim.ActivePiece));
    }

    private static Level CreateTestLevel() =>
        new(
            new("lock-delay", "Lock Delay Golden", "0.2.0", 1U),
            new(20, 20, 20),
            [],
            [],
            new(),
            new("FIXED_SEQUENCE", ["I4"], null),
            []
        );

    private static Simulation CreateSimulationWithGroundPlane()
    {
        Simulation sim = new(CreateTestLevel(), new Pcg32(1));
        ActivePiece piece = sim.ActivePiece!;
        int supportY = piece.Origin.Y - 1;
        Assert.True(supportY >= 0);

        for (int x = 0; x < sim.Grid.Size.X; x++)
        {
            for (int z = 0; z < sim.Grid.Size.Z; z++)
            {
                sim.Grid.SetVoxel(new Int3(x, supportY, z), new Voxel(OccupancyType.Bedrock));
            }
        }

        return sim;
    }

    private static void TickNTimes(Simulation sim, InputCommand command, int count)
    {
        if (count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            sim.Tick(command);
        }
    }
}
