namespace Floodline.Core.Tests;

public class PrimitiveTests
{
    [Fact]
    public void Int3AdditionIsCorrect()
    {
        Int3 a = new(1, 2, 3);
        Int3 b = new(4, 5, 6);
        Assert.Equal(new Int3(5, 7, 9), a + b);
    }

    [Fact]
    public void GravityVectorsAreCorrect()
    {
        Assert.Equal(new Int3(0, -1, 0), GravityTable.GetVector(GravityDirection.Down));
        Assert.Equal(new Int3(0, 0, -1), GravityTable.GetVector(GravityDirection.North));
        Assert.Equal(new Int3(1, 0, 0), GravityTable.GetVector(GravityDirection.East));
    }

    [Fact]
    public void DeterministicOrderingIsStable()
    {
        GravityDirection dir = GravityDirection.Down; // Up is (0,1,0)
        Int3 c1 = new(0, 0, 0);
        Int3 c2 = new(1, 0, 0);
        Int3 c3 = new(0, 1, 0);

        Comparison<Int3> comparison = DeterministicOrdering.GetComparison(dir);

        // c1 vs c3: c3 is at depth -1 (Y=1), c1 is at depth 0 (Y=0).
        // Sorting GravElev ascending: -1 < 0, so c3 < c1
        Assert.True(comparison(c3, c1) < 0);

        // c1 vs c2: same elev (0), tie-break: R is (1,0,0), so c2 has R=1, c1 has R=0. c1 < c2
        Assert.True(comparison(c1, c2) < 0);
    }

    [Fact]
    public void RightVectorsMatchSpec()
    {
        // Spec 2.4.1 table:
        // DOWN: R=(1,0,0)
        Assert.Equal(new Int3(1, 0, 0), GravityTable.GetRightVector(GravityDirection.Down));
        // NORTH (0,0,-1): U=(0,0,1), R=(1,0,0)
        Assert.Equal(new Int3(1, 0, 0), GravityTable.GetRightVector(GravityDirection.North));
        // EAST (1,0,0): U=(-1,0,0), R=(0,0,1)
        Assert.Equal(new Int3(0, 0, 1), GravityTable.GetRightVector(GravityDirection.East));
    }

    [Fact]
    public void GravityRotationIsCorrect()
    {
        // Start with Down (0, -1, 0)
        // Tilt Forward (PitchCW) -> rotates Down (Y=-1) to North (Z=-1)
        Assert.Equal(GravityDirection.North, GravityTable.GetRotatedGravity(GravityDirection.Down, Matrix3x3.PitchCW));

        // Start with South (0, 0, 1)
        // Tilt Right (RollCCW) -> rotates Z to -X? No, Roll is around Z.
        // Wait, RollCCW transforms X to -Y. 
        // Let's re-verify: RollCCW = (0, 1, 0, -1, 0, 0, 0, 0, 1)
        // [0 1 0] [0]   [0]
        // [-1 0 0] [0] = [0]
        // [0 0 1] [1]   [1]
        // (0,0,1) transformed by RollCCW is (0,0,1). Correct, rotation around Z doesn't change Z.

        // Let's try Tilt Left (RollCW) from East (1, 0, 0)
        // RollCW = (0, -1, 0, 1, 0, 0, 0, 0, 1)
        // [0 -1 0] [1]   [0]
        // [1 0 0] [0] = [1]
        // [0 0 1] [0]   [0]
        // Gravity (1,0,0) becomes (0,1,0) which is Up.
        Assert.Null(GravityTable.GetRotatedGravity(GravityDirection.East, Matrix3x3.RollCW));
    }

    [Fact]
    public void TieCoordMappingMatchesSpec()
    {
        // Spec Table §2.1.2:
        // Down (0,-1,0)  => U=-Y, R=X,  F=Z
        // North (0,0,-1) => U=-Z, R=X,  F=-Y
        // South (0,0,1)  => U=Z,  R=X,  F=Y
        // East (1,0,0)   => U=X,  R=Z,  F=-Y
        // West (-1,0,0)  => U=-X, R=Z,  F=Y

        Int3 pos = new(1, 2, 3);

        // Down: U=-2, R=1, F=3
        Assert.Equal(new TieCoord(-2, 1, 3), DeterministicOrdering.GetTieCoord(pos, GravityDirection.Down));

        // North: U=-3, R=1, F=-2
        Assert.Equal(new TieCoord(-3, 1, -2), DeterministicOrdering.GetTieCoord(pos, GravityDirection.North));

        // South: U=3, R=1, F=2
        Assert.Equal(new TieCoord(3, 1, 2), DeterministicOrdering.GetTieCoord(pos, GravityDirection.South));

        // East: U=1, R=3, F=-2
        Assert.Equal(new TieCoord(1, 3, -2), DeterministicOrdering.GetTieCoord(pos, GravityDirection.East));

        // West: U=-1, R=3, F=2
        Assert.Equal(new TieCoord(-1, 3, 2), DeterministicOrdering.GetTieCoord(pos, GravityDirection.West));
    }
}
