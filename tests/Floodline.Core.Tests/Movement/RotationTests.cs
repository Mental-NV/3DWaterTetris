using Floodline.Core.Movement;

namespace Floodline.Core.Tests.Movement;

public class RotationTests
{
    private readonly Grid _grid;
    private readonly MovementController _controller;

    public RotationTests()
    {
        _grid = new Grid(new Int3(10, 20, 10));
        _controller = new MovementController(_grid);
    }

    [Fact]
    public void AttemptRotation_ValidSpace_RotatesPiece()
    {
        // I3: (0,0,0), (1,0,0), (2,0,0)
        PieceDefinition i3 = PieceLibrary.Get(PieceId.I3);
        OrientedPiece piece = new(i3.Id, i3.UniqueOrientations[0], 0);
        _controller.CurrentPiece = new ActivePiece(piece, new Int3(5, 5, 5));

        // Rotate Yaw CW. Should move from X-axis to Z-axis (either + or -).
        bool success = _controller.ProcessInput(InputCommand.RotatePieceYawCW).Moved;

        Assert.True(success);

        // Check alignment: all voxels should have X=0 and Y=0 relative to pivot
        IReadOnlyList<Int3> voxels = _controller.CurrentPiece.Piece.Voxels;
        foreach (Int3 v in voxels)
        {
            Assert.Equal(0, v.X);
            Assert.Equal(0, v.Y);
        }
    }

    [Fact]
    public void AttemptRotation_BlockedByWall_Kicks()
    {
        // We want to force a kick. 
        // Use I4 along X: (0,0,0), (1,0,0), (2,0,0), (3,0,0).
        // Place it so it's valid along X, but if it rotates to Z it's OOB.
        // OR better: block origin with another piece, forcing a kick.

        PieceDefinition i4 = PieceLibrary.Get(PieceId.I4);
        OrientedPiece pieceHorizontal = new(i4.Id, i4.UniqueOrientations[0], 0); // Along X

        // Find the orientation for Z axis
        OrientedPiece pieceVertical = PieceLibrary.Rotate(pieceHorizontal, Matrix3x3.YawCW);

        // Setup: Place I4 at (5, 5, 5). 
        _controller.CurrentPiece = new ActivePiece(pieceHorizontal, new Int3(5, 5, 5));

        // Block the origin (5, 5, 5) and (5, 5, 6) etc with Walls.
        // Wait, if I block the origin, the piece can't be THERE.
        // Let's block where it WOULD rotate to.

        // I4 Horizontal: (5,5,5), (6,5,5), (7,5,5), (8,5,5).
        // Rotate to Z at (5,5,5) -> (5,5,5) and some Z offsets.
        // Block (5,5,5) area? No, the piece is already there.

        // Let's use a wall at X=5.
        // I4 along Z: (5,5,5), (5,5,6), (5,5,7), (5,5,8).
        // Rotate to X: (5,5,5), (6,5,5), (7,5,5), (8,5,5).
        // If we block (5,5,5) TO (8,5,5), it must kick.

        _controller.CurrentPiece = new ActivePiece(pieceVertical, new Int3(5, 5, 5));

        // Block the target region (X=5..8, Y=5, Z=5)
        for (int x = 5; x <= 8; x++)
        {
            _grid.SetVoxel(new Int3(x, 5, 5), new Voxel(OccupancyType.Wall));
        }

        // Wait, if I block (5,5,5), the piece I just placed is in collision!
        // TryTranslate/AttemptRotation would fail if start is invalid.
        // So I should block adjacent to the piece.

        // Reset grid
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    _grid.SetVoxel(new Int3(x, y, z), Voxel.Empty);
                }
            }
        }

        // I4 along X at (5,5,5) works.
        _controller.CurrentPiece = new ActivePiece(pieceHorizontal, new Int3(5, 5, 5));

        // Now suppose we rotate to Z. 
        // Target Z voxels: (5,5,5), (5,5,6), (5,5,7), (5,5,8) etc.
        // Block (5,5,5) - can't, piece is there.
        // Block (5,5,6) - yes.
        _grid.SetVoxel(new Int3(5, 5, 6), new Voxel(OccupancyType.Wall));
        _grid.SetVoxel(new Int3(5, 5, 4), new Voxel(OccupancyType.Wall)); // Block -Z too

        // Now rotation to Z is blocked at origin.
        // It must kick.
        // Kick +X (1,0,0) -> Origin (6,5,5). 
        // Z voxels: (6,5,5), (6,5,6), (6,5,7), (6,5,8).
        // If these are empty, kick succeeds!

        bool success = _controller.ProcessInput(InputCommand.RotatePieceYawCW).Moved;
        Assert.True(success, "Kick should have succeeded");
        Assert.NotEqual(new Int3(5, 5, 5), _controller.CurrentPiece.Origin);
    }

    [Fact]
    public void AttemptRotation_FullyBlocked_ReturnsFalse()
    {
        // Place a piece and surround it with Bedrock.
        PieceDefinition o2 = PieceLibrary.Get(PieceId.O2);
        OrientedPiece piece = new(o2.Id, o2.UniqueOrientations[0], 0);
        Int3 origin = new(5, 5, 5);
        _controller.CurrentPiece = new ActivePiece(piece, origin);

        // Fill the entire grid with Bedrock EXCEPT where the piece currently is.
        // O2 is (0,0,0), (1,0,0), (0,0,1), (1,0,1).
        HashSet<Int3> pieceVoxels = [origin, origin + new Int3(1, 0, 0), origin + new Int3(0, 0, 1), origin + new Int3(1, 0, 1)];

        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    Int3 pos = new(x, y, z);
                    if (!pieceVoxels.Contains(pos))
                    {
                        _grid.SetVoxel(pos, new Voxel(OccupancyType.Bedrock));
                    }
                }
            }
        }

        // Now try ANY rotation. 
        // Even if O2 is symmetric, it will try to match a UniqueOrientation.
        // And even if it's the same shape, it will try to place it.
        // But here it should fail if any rotation/kick hits the bedrock.
        // Wait, if O2 rotates to the SAME voxels, it will SUCCEED.
        // I need a piece that DEFINITELY changes voxels on rotation.

        PieceDefinition i3 = PieceLibrary.Get(PieceId.I3); // (0,0,0), (1,0,0), (2,0,0)
        piece = new(i3.Id, i3.UniqueOrientations[0], 0);
        _controller.CurrentPiece = new ActivePiece(piece, origin);
        pieceVoxels = [origin, origin + new Int3(1, 0, 0), origin + new Int3(2, 0, 0)];

        // Refill grid
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    Int3 pos = new(x, y, z);
                    _grid.SetVoxel(pos, pieceVoxels.Contains(pos) ? Voxel.Empty : new Voxel(OccupancyType.Bedrock));
                }
            }
        }

        // Try to rotate. Every other orientation and every kick is blocked by Bedrock.
        bool success = _controller.ProcessInput(InputCommand.RotatePieceYawCW).Moved;
        Assert.False(success);
    }
}
