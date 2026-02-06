using Floodline.Core;
using Floodline.Core.Movement;
using UnityEngine;

namespace Floodline.Client
{
    /// <summary>
    /// Orchestrates input sampling, buffering, and per-tick command generation.
    /// - Manages switching between device and scripted input sources
    /// - Applies DAS/ARR and lock delay logic
    /// - Converts to Core InputCommand enums
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// The active input source (device or scripted)
        /// </summary>
        public InputSource? InputSource { get; set; }

        /// <summary>
        /// Configuration for input timing (DAS, ARR, lock delay, etc.)
        /// </summary>
        public InputBuffer.Config BufferConfig { get; set; } = new();

        /// <summary>
        /// The input buffer (exposed for testing)
        /// </summary>
        public InputBuffer Buffer => buffer;

        // Internal buffering state
        private InputBuffer buffer;


        // Recorded command history for test fixtures
        private System.Collections.Generic.List<InputCommand> commandHistory = new();

        private void Awake()
        {
            buffer = new InputBuffer(BufferConfig);

            // If no input source set, try to find DeviceInputSource on this GameObject
            if (InputSource == null)
            {
                InputSource = GetComponent<DeviceInputSource>();
            }

            // If still no source, create one
            if (InputSource == null)
            {
                InputSource = gameObject.AddComponent<DeviceInputSource>();
            }
        }

        /// <summary>
        /// Called when a new piece spawns.
        /// Resets buffering state for clean DAS/ARR and lock delay tracking.
        /// </summary>
        public void OnNewPieceSpawned()
        {
            buffer.ResetPiece();
        }

        /// <summary>
        /// Sample input, apply buffering, and generate a single InputCommand for this tick.
        /// Called once per simulation tick (60 Hz).
        /// </summary>
        public InputCommand SampleAndGenerateCommand()
        {
            if (InputSource == null)
                return InputCommand.None;

            // Sample raw device input
            RawInputState rawInput = InputSource.SampleInput();

            // Decrement cooldowns (e.g., world rotation cooldown)
            buffer.TickCooldowns();

            // Map to InputCommand
            InputCommand cmd = InputMapper.MapToCommand(rawInput, buffer);

            // Record for playback/debugging
            commandHistory.Add(cmd);

            return cmd;
        }

        /// <summary>
        /// Update lock delay state based on piece groundedness.
        /// </summary>
        /// <param name="isGrounded">Whether the active piece is grounded.</param>
        /// <returns>true if piece should lock immediately due to timeout.</returns>
        public bool UpdateLockDelay(bool isGrounded)
        {
            return buffer.UpdateLockDelay(isGrounded);
        }

        /// <summary>
        /// Get the recorded command history.
        /// Useful for replays and test assertions.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<InputCommand> CommandHistory => commandHistory.AsReadOnly();

        /// <summary>
        /// Clear command history
        /// </summary>
        public void ClearHistory()
        {
            commandHistory.Clear();
        }

        /// <summary>
        /// Switch to a scripted input source for testing.
        /// </summary>
        public void UseScriptedInput(string jsonFixture)
        {
            var scriptedSource = gameObject.AddComponent<ScriptedInputSource>();
            scriptedSource.LoadFromJson(jsonFixture);
            InputSource = scriptedSource;
            buffer.ResetPiece();
            ClearHistory();
        }

        /// <summary>
        /// Reset to device input.
        /// </summary>
        public void UseDeviceInput()
        {
            InputSource = GetComponent<DeviceInputSource>() ?? gameObject.AddComponent<DeviceInputSource>();
            buffer.ResetPiece();
            ClearHistory();
        }
    }
}
