namespace Floodline.Core.Movement;

/// <summary>
/// Processes per-tick input commands and applies gravity to the active piece.
/// Implements movement logic per Input_Feel_v0_2.md §2.
/// </summary>
/// <param name="grid">The grid.</param>
public sealed class MovementController(Grid grid)
{
    /// <summary>
    /// Gets or sets the current active piece.
    /// </summary>
    public ActivePiece? CurrentPiece { get; set; }

    /// <summary>
    /// Gets the grid.
    /// </summary>
    public Grid Grid { get; } = grid ?? throw new ArgumentNullException(nameof(grid));

    /// <summary>
    /// Gets the current world gravity direction.
    /// Default is Down (-Y).
    /// </summary>
    public GravityDirection Gravity { get; private set; } = GravityDirection.Down;

    /// <summary>
    /// Sets the current world gravity direction.
    /// </summary>
    /// <param name="gravity">The new gravity direction.</param>
    public void SetGravity(GravityDirection gravity) => Gravity = gravity;

    /// <summary>
    /// Processes a single input command for the current tick.
    /// Per Input_Feel_v0_2.md §2, commands are applied in canonical order.
    /// </summary>
    /// <param name="command">The input command to process.</param>
    /// <returns>True if the command was successfully applied; otherwise, false.</returns>
    public bool ProcessInput(InputCommand command) =>
        CurrentPiece is not null && command switch
        {
            InputCommand.MoveLeft => CurrentPiece.TryTranslate(new Int3(-1, 0, 0), Grid),
            InputCommand.MoveRight => CurrentPiece.TryTranslate(new Int3(1, 0, 0), Grid),
            InputCommand.MoveForward => CurrentPiece.TryTranslate(new Int3(0, 0, -1), Grid),
            InputCommand.MoveBack => CurrentPiece.TryTranslate(new Int3(0, 0, 1), Grid),
            InputCommand.SoftDrop => ApplyGravityStep(),
            InputCommand.HardDrop => ApplyHardDrop(),
            InputCommand.RotatePiece => false, // Placeholder for FL-0107
            InputCommand.RotateWorld => false, // Placeholder for FL-0108
            InputCommand.None => true,
            _ => false
        };

    /// <summary>
    /// Applies a single gravity step to the active piece.
    /// Moves the piece one cell in the gravity direction if possible.
    /// </summary>
    /// <returns>True if the piece moved; false if blocked (lock condition).</returns>
    public bool ApplyGravityStep()
    {
        if (CurrentPiece is null)
        {
            return false;
        }

        Int3 gravityVector = GravityTable.GetVector(Gravity);
        return CurrentPiece.TryTranslate(gravityVector, Grid);
    }

    /// <summary>
    /// Applies hard drop: moves the piece as far as possible in gravity direction.
    /// Per Input_Feel_v0_2.md §4.4, hard drop locks immediately (no lock delay).
    /// </summary>
    /// <returns>True if at least one step was taken; otherwise, false.</returns>
    private bool ApplyHardDrop()
    {
        if (CurrentPiece is null)
        {
            return false;
        }

        bool movedAtLeastOnce = false;
        while (ApplyGravityStep())
        {
            movedAtLeastOnce = true;
        }

        return movedAtLeastOnce;
    }
}
