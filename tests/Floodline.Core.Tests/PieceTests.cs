
namespace Floodline.Core.Tests;

public class PieceTests
{
    [Fact]
    public void PieceLibraryContainsAllTwelvePieces()
    {
        List<PieceDefinition> pieces = [.. PieceLibrary.All()];
        Assert.Equal(12, pieces.Count);
    }

    [Theory]
    [InlineData(PieceId.O2, 4)]
    [InlineData(PieceId.I3, 3)]
    [InlineData(PieceId.I4, 4)]
    [InlineData(PieceId.L3, 3)]
    [InlineData(PieceId.L4, 4)]
    [InlineData(PieceId.J4, 4)]
    [InlineData(PieceId.T3, 4)]
    [InlineData(PieceId.S4, 4)]
    [InlineData(PieceId.Z4, 4)]
    [InlineData(PieceId.U5, 5)]
    [InlineData(PieceId.P5, 5)]
    [InlineData(PieceId.C3D5, 5)]
    public void PieceLibraryPieceVoxelCountsMatchDefinitions(PieceId id, int expectedCount)
    {
        PieceDefinition piece = PieceLibrary.Get(id);
        Assert.Equal(expectedCount, piece.Voxels.Count);
    }

    [Fact]
    public void PieceLibraryAllPiecesContainPivot()
    {
        foreach (PieceDefinition piece in PieceLibrary.All())
        {
            Assert.Contains(new Int3(0, 0, 0), piece.Voxels);
        }
    }

    [Fact]
    public void Matrix3x3GeneratesTwentyFourUniqueRotations() => Assert.Equal(24, Matrix3x3.AllRotations.Count);

    [Theory]
    [InlineData(PieceId.O2, 3)] // 2x2 square has 3 unique orientations (XY, XZ, YZ planes)
    [InlineData(PieceId.I3, 3)] // Line can be on X, Y, or Z axis
    [InlineData(PieceId.I4, 3)]
    public void OrientationGeneratorProducesExpectedUniqueCounts(PieceId id, int expectedUnique)
    {
        PieceDefinition piece = PieceLibrary.Get(id);
        IReadOnlyList<IReadOnlyList<Int3>> unique = OrientationGenerator.GetUniqueOrientations(piece.Voxels);
        Assert.Equal(expectedUnique, unique.Count);
    }

    [Fact]
    public void OrientationGeneratorAllOrientationsContainPivotAtPivot()
    {
        // Pivot rule: pivot is at (0,0,0) and stays at (0,0,0) after rotation.
        // It is NOT re-centered per Content_Pack_v0_2 Section 1.2
        foreach (PieceDefinition piece in PieceLibrary.All())
        {
            IReadOnlyList<IReadOnlyList<Int3>> unique = OrientationGenerator.GetUniqueOrientations(piece.Voxels);
            foreach (IReadOnlyList<Int3> orientationVoxels in unique)
            {
                // In my implementation, Matrix3x3.Transform rotates relative to origin (0,0,0).
                // Since (0,0,0) is in piece.Voxels and R * 0 = 0, (0,0,0) must always be present.
                Assert.Contains(new Int3(0, 0, 0), orientationVoxels);
            }
        }
    }
}
