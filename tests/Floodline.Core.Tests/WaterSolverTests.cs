using Floodline.Core;
using Xunit;

namespace Floodline.Core.Tests;

public class WaterSolverTests
{
    [Fact]
    public void Settle_Fills_Lowest_Cells_In_Column()
    {
        Grid grid = new(new Int3(1, 3, 1));
        grid.SetVoxel(new Int3(0, 2, 0), Voxel.Water);
        grid.SetVoxel(new Int3(0, 1, 0), Voxel.Water);

        WaterSettleResult result = WaterSolver.Settle(grid, GravityDirection.Down, []);

        Assert.Equal(2, result.TotalUnits);
        Assert.Equal(0, result.OverflowUnits);
        Assert.Equal(OccupancyType.Water, grid.GetVoxel(new Int3(0, 0, 0)).Type);
        Assert.Equal(OccupancyType.Water, grid.GetVoxel(new Int3(0, 1, 0)).Type);
        Assert.Equal(OccupancyType.Empty, grid.GetVoxel(new Int3(0, 2, 0)).Type);
    }

    [Fact]
    public void Settle_Spills_Over_Ridge()
    {
        Grid grid = new(new Int3(3, 2, 1));
        grid.SetVoxel(new Int3(1, 0, 0), new Voxel(OccupancyType.Wall));
        grid.SetVoxel(new Int3(0, 0, 0), Voxel.Water);
        grid.SetVoxel(new Int3(0, 1, 0), Voxel.Water);

        WaterSettleResult result = WaterSolver.Settle(grid, GravityDirection.Down, []);

        Assert.Equal(2, result.TotalUnits);
        Assert.Equal(OccupancyType.Water, grid.GetVoxel(new Int3(0, 0, 0)).Type);
        Assert.Equal(OccupancyType.Water, grid.GetVoxel(new Int3(2, 0, 0)).Type);
        Assert.Equal(OccupancyType.Empty, grid.GetVoxel(new Int3(0, 1, 0)).Type);
    }

    [Fact]
    public void Settle_Uses_Displaced_Sources_As_Water_Units()
    {
        Grid grid = new(new Int3(1, 1, 1));
        Int3 displaced = new(0, 0, 0);
        Int3[] displacedSources = [displaced];

        WaterSettleResult result = WaterSolver.Settle(grid, GravityDirection.Down, displacedSources);

        Assert.Equal(1, result.TotalUnits);
        Assert.Equal(1, result.FilledUnits);
        Assert.Equal(0, result.OverflowUnits);
        Assert.Equal(OccupancyType.Water, grid.GetVoxel(displaced).Type);
    }
}
