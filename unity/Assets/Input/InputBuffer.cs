using Floodline.Core;
using UnityEngine;

namespace Floodline.Client
{
    /// <summary>
    /// Input buffering and timing logic.
    /// Implements DAS/ARR (Delayed Auto-Shift / Auto-Repeat Rate) for movement
    /// Lock delay tracking, and determines per-tick commands.
    /// All timing is in integer ticks (1 tick = 1/60 sec).
    /// </summary>
    public class InputBuffer
    {
        /// <summary>
        /// Configuration for input timing
        /// </summary>
        public class Config
        {
            public int DAS = 10;  // Initial delay before repeat (ticks) ≈166ms @ 60Hz
            public int ARR = 2;   // Repeat rate (ticks between repeats) ≈33ms @ 60Hz
            public int softDropRate = 1;  // Cells per tick while soft-dropping
            public int lockDelayTicks = 12;  // Ticks before forced lock ≈200ms @ 60Hz
            public int maxLockResets = 4;  // Max times piece can reset lock delay
            public int worldRotateCooldwonTicks = 0;  // Optional cooldown between rotations
        }

        private Config config;

        // Movement repeat state (DAS/ARR)
        private int moveXRepeatTimer = -1;  // -1 = not active, else countdown
        private int moveZRepeatTimer = -1;
        private int lastMoveXInput = 0;     // Persist direction for DAS restart on change
        private int lastMoveZInput = 0;

        // Lock delay state
        private int lockDelayTimer = -1;  // -1 = not active, else countdown
        private int lockResetCount = 0;   // Current piece's lock reset count
        private bool wasGrounded = false; // Detect grounded→ungrounded transition

        // World rotation cooldown
        private int worldRotateCooldown = 0;

        public InputBuffer(Config config = null)
        {
            this.config = config ?? new Config();
        }

        /// <summary>
        /// Reset state for a new piece drop
        /// </summary>
        public void ResetPiece()
        {
            moveXRepeatTimer = -1;
            moveZRepeatTimer = -1;
            lastMoveXInput = 0;
            lastMoveZInput = 0;
            lockDelayTimer = -1;
            lockResetCount = 0;
            wasGrounded = false;
            worldRotateCooldown = 0;
        }

        /// <summary>
        /// Tick decrement world rotation cooldown
        /// </summary>
        public void TickCooldowns()
        {
            if (worldRotateCooldown > 0)
                worldRotateCooldown--;
        }

        /// <summary>
        /// Update lock delay state based on whether piece is grounded.
        /// If piece becomes ungrounded, resets lock delay timer (up to max resets).
        /// Also handles forced lock when timer expires.
        /// </summary>
        /// <returns>true if piece should lock immediately</returns>
        public bool UpdateLockDelay(bool isGrounded)
        {
            // Check for grounded→ungrounded transition (lock reset)
            if (wasGrounded && !isGrounded)
            {
                if (lockResetCount < config.maxLockResets)
                {
                    lockResetCount++;
                    lockDelayTimer = config.lockDelayTicks;  // Reset timer
                }
            }

            // Update grounded state
            wasGrounded = isGrounded;

            // If piece just became grounded, start lock delay if not already active
            if (isGrounded && lockDelayTimer == -1)
            {
                lockDelayTimer = config.lockDelayTicks;
            }

            // Decrement lock delay timer
            if (lockDelayTimer > 0)
            {
                lockDelayTimer--;
                if (lockDelayTimer == 0)
                {
                    lockDelayTimer = -1;
                    return true;  // Force lock
                }
            }

            return false;
        }

        /// <summary>
        /// Process raw input and generate per-tick movement commands.
        /// Applies DAS/ARR logic for repeating movement.
        /// </summary>
        public (int moveX, int moveZ, bool softDrop) ProcessMovement(RawInputState raw)
        {
            int outX = 0;
            int outZ = 0;

            // Horizontal movement (X)
            if (raw.moveX != 0)
            {
                if (raw.moveX != lastMoveXInput)
                {
                    // Direction changed, send immediately and reset DAS
                    outX = raw.moveX;
                    moveXRepeatTimer = config.DAS;
                    lastMoveXInput = raw.moveX;
                }
                else if (moveXRepeatTimer == 0)
                {
                    // ARR tick fire
                    outX = raw.moveX;
                    moveXRepeatTimer = config.ARR;
                }
                else if (moveXRepeatTimer == -1)
                {
                    // Just activated
                    outX = raw.moveX;
                    moveXRepeatTimer = config.DAS;
                    lastMoveXInput = raw.moveX;
                }
            }
            else
            {
                moveXRepeatTimer = -1;
                lastMoveXInput = 0;
            }

            // Vertical movement (Z)
            if (raw.moveZ != 0)
            {
                if (raw.moveZ != lastMoveZInput)
                {
                    // Direction changed, send immediately and reset DAS
                    outZ = raw.moveZ;
                    moveZRepeatTimer = config.DAS;
                    lastMoveZInput = raw.moveZ;
                }
                else if (moveZRepeatTimer == 0)
                {
                    // ARR tick fire
                    outZ = raw.moveZ;
                    moveZRepeatTimer = config.ARR;
                }
                else if (moveZRepeatTimer == -1)
                {
                    // Just activated
                    outZ = raw.moveZ;
                    moveZRepeatTimer = config.DAS;
                    lastMoveZInput = raw.moveZ;
                }
            }
            else
            {
                moveZRepeatTimer = -1;
                lastMoveZInput = 0;
            }

            // Decrement timers (this happens every tick, even if no input)
            if (moveXRepeatTimer > 0) moveXRepeatTimer--;
            if (moveZRepeatTimer > 0) moveZRepeatTimer--;

            return (outX, outZ, raw.softDrop);
        }

        /// <summary>
        /// Check if world rotation is allowed (cooldown not active)
        /// </summary>
        public bool CanRotateWorld()
        {
            return worldRotateCooldown == 0;
        }

        /// <summary>
        /// Activate world rotation cooldown
        /// </summary>
        public void ExecuteWorldRotation()
        {
            worldRotateCooldown = config.worldRotateCooldwonTicks;
        }
    }
}
