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

        // c1 vs c3: c3 is higher elev (1 vs 0), so c1 < c3
        Assert.True(comparison(c1, c3) < 0);

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
}
