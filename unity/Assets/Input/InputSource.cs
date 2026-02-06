using System;
using UnityEngine;

namespace Floodline.Client
{
    /// <summary>
    /// Abstract input source for device inputs.
    /// Decouples device input sampling from input buffering logic.
    /// Supports both real device input and scripted input for testing.
    /// </summary>
    public abstract class InputSource : MonoBehaviour
    {
        /// <summary>
        /// Samples raw device input state at the current frame.
        /// Returns a snapshot of currently pressed inputs (buttons, axes).
        /// </summary>
        public abstract RawInputState SampleInput();

        /// <summary>
        /// Optional: reset input state (e.g., on test fixture reload)
        /// </summary>
        public virtual void ResetInput() { }
    }

    /// <summary>
    /// Snapshot of raw device input at a single point in time.
    /// </summary>
    [Serializable]
    public class RawInputState
    {
        /// <summary>
        /// Horizontal movement: -1 (left), 0 (neutral), +1 (right)
        /// Combine A/D or left/right arrow keys
        /// </summary>
        public int moveX;

        /// <summary>
        /// Vertical (forward/back) movement: -1 (forward/north), 0 (neutral), +1 (back/south)
        /// Combine W/S or up/down arrow keys
        /// </summary>
        public int moveZ;

        /// <summary>
        /// Piece rotation axes: each -1, 0, or +1
        /// </summary>
        public int rotateYaw;  // Q/E
        public int rotatePitch;  // R/F
        public int rotateRoll;  // Z/C

        /// <summary>
        /// Soft drop (increases gravity)
        /// </summary>
        public bool softDrop;

        /// <summary>
        /// Hard drop (immediate place+lock)
        /// </summary>
        public bool hardDrop;

        /// <summary>
        /// Hold (next piece preview / stash)
        /// </summary>
        public bool holdAction;

        /// <summary>
        /// World rotation axes
        /// </summary>
        public int rotateWorldForward;  // +1 for forward tilt, -1 for back tilt
        public int rotateWorldLeft;     // +1 for left tilt, -1 for right tilt

        /// <summary>
        /// Ability actions (discrete toggles)
        /// </summary>
        public bool abilityStabilize;
        public bool abilityFreeze;
        public bool abilityDrain;

        /// <summary>
        /// Camera view snap requests
        /// </summary>
        public int cameraNE;  // F1
        public int cameraNW;  // F2
        public int cameraSE;  // F3
        public int cameraSW;  // F4

        /// <summary>
        /// Clear all inputs
        /// </summary>
        public void Clear()
        {
            moveX = 0;
            moveZ = 0;
            rotateYaw = 0;
            rotatePitch = 0;
            rotateRoll = 0;
            softDrop = false;
            hardDrop = false;
            holdAction = false;
            rotateWorldForward = 0;
            rotateWorldLeft = 0;
            abilityStabilize = false;
            abilityFreeze = false;
            abilityDrain = false;
            cameraNE = 0;
            cameraNW = 0;
            cameraSE = 0;
            cameraSW = 0;
        }
    }
}
