using Floodline.Core.Movement;

namespace Floodline.Client
{
    /// <summary>
    /// Maps raw input state and buffering decisions to Core InputCommand enum values.
    /// Applies the input model from Input_Feel_v0_2.md section 2 in canonical order:
    /// 1. World rotate
    /// 2. Ability actions
    /// 3. Piece rotate
    /// 4. Piece move (with DAS/ARR)
    /// 5. Soft drop
    /// 6. Hard drop
    /// 
    /// Note: Each tick produces ONE InputCommand enum value.
    /// DAS/ARR logic deterministically prevents simultaneous moves on the same tick.
    /// </summary>
    public static class InputMapper
    {
        /// <summary>
        /// Map raw input and buffered state to a single InputCommand.
        /// Priority order: world rotation > abilities > piece rotation > movement > drop
        /// </summary>
        public static InputCommand MapToCommand(
            RawInputState raw,
            InputBuffer buffer
        )
        {
            // 1. World rotation (highest priority, mutually exclusive)
            if (buffer.CanRotateWorld())
            {
                if (raw.rotateWorldForward != 0)
                {
                    buffer.ExecuteWorldRotation();
                    return raw.rotateWorldForward > 0
                        ? InputCommand.RotateWorldForward
                        : InputCommand.RotateWorldBack;
                }
                else if (raw.rotateWorldLeft != 0)
                {
                    buffer.ExecuteWorldRotation();
                    return raw.rotateWorldLeft > 0
                        ? InputCommand.RotateWorldLeft
                        : InputCommand.RotateWorldRight;
                }
            }

            // 2. Ability actions (discrete, one per tick max)
            if (raw.abilityStabilize)
                return InputCommand.Stabilize;
            else if (raw.abilityFreeze)
                return InputCommand.FreezeAbility;
            else if (raw.abilityDrain)
                return InputCommand.DrainPlacementAbility;

            // 3. Piece rotation (one axis per tick)
            if (raw.rotateYaw != 0)
                return raw.rotateYaw > 0 ? InputCommand.RotatePieceYawCW : InputCommand.RotatePieceYawCCW;
            else if (raw.rotatePitch != 0)
                return raw.rotatePitch > 0 ? InputCommand.RotatePiecePitchCW : InputCommand.RotatePiecePitchCCW;
            else if (raw.rotateRoll != 0)
                return raw.rotateRoll > 0 ? InputCommand.RotatePieceRollCW : InputCommand.RotatePieceRollCCW;

            // 4. Piece movement (with DAS/ARR applied by InputBuffer)
            var (moveX, moveZ, softDrop) = buffer.ProcessMovement(raw);
            
            // Movement priority: prefer X over Z (arbitrary choice for diagonal ambiguity)
            if (moveX != 0)
                return moveX > 0 ? InputCommand.MoveRight : InputCommand.MoveLeft;
            else if (moveZ != 0)
                return moveZ > 0 ? InputCommand.MoveBack : InputCommand.MoveForward;

            // 5. Soft drop
            if (softDrop)
                return InputCommand.SoftDrop;

            // 6. Hard drop
            if (raw.hardDrop)
                return InputCommand.HardDrop;

            // 7. Hold
            if (raw.holdAction)
                return InputCommand.Hold;

            // No input
            return InputCommand.None;
        }
    }
}
