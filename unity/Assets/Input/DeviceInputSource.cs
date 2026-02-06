using UnityEngine;

namespace Floodline.Client
{
    /// <summary>
    /// Real device input source that reads from Unity Input System (legacy Input class).
    /// Hardcoded to PC keyboard layout per Input_Feel_v0_2.md section 8.
    /// </summary>
    public class DeviceInputSource : InputSource
    {
        public override RawInputState SampleInput()
        {
            var state = new RawInputState();

            // Movement: A/D (X), W/S (Z)
            if (Input.GetKey(KeyCode.A))
                state.moveX = -1;
            else if (Input.GetKey(KeyCode.D))
                state.moveX = 1;

            if (Input.GetKey(KeyCode.W))
                state.moveZ = -1;
            else if (Input.GetKey(KeyCode.S))
                state.moveZ = 1;

            // Piece rotation: Q/E (yaw), R/F (pitch), Z/C (roll)
            if (Input.GetKey(KeyCode.Q))
                state.rotateYaw = -1;
            else if (Input.GetKey(KeyCode.E))
                state.rotateYaw = 1;

            if (Input.GetKey(KeyCode.R))
                state.rotatePitch = 1;
            else if (Input.GetKey(KeyCode.F))
                state.rotatePitch = -1;

            if (Input.GetKey(KeyCode.Z))
                state.rotateRoll = -1;
            else if (Input.GetKey(KeyCode.C))
                state.rotateRoll = 1;

            // Soft drop: Left Ctrl
            state.softDrop = Input.GetKey(KeyCode.LeftControl);

            // Hard drop: Space
            state.hardDrop = Input.GetKeyDown(KeyCode.Space);

            // Hold: Left Shift
            state.holdAction = Input.GetKey(KeyCode.LeftShift);

            // World rotation: 1/2/3/4
            if (Input.GetKeyDown(KeyCode.Alpha1))
                state.rotateWorldForward = 1;  // Tilt forward
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                state.rotateWorldForward = -1;  // Tilt back

            if (Input.GetKeyDown(KeyCode.Alpha3))
                state.rotateWorldLeft = -1;  // Tilt right
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                state.rotateWorldLeft = 1;   // Tilt left

            // Abilities: V (Stabilize), B (Freeze), N (Drain)
            state.abilityStabilize = Input.GetKeyDown(KeyCode.V);
            state.abilityFreeze = Input.GetKeyDown(KeyCode.B);
            state.abilityDrain = Input.GetKeyDown(KeyCode.N);

            // Camera snaps: F1-F4
            if (Input.GetKeyDown(KeyCode.F1))
                state.cameraNE = 1;
            else if (Input.GetKeyDown(KeyCode.F2))
                state.cameraNW = 1;
            else if (Input.GetKeyDown(KeyCode.F3))
                state.cameraSE = 1;
            else if (Input.GetKeyDown(KeyCode.F4))
                state.cameraSW = 1;

            return state;
        }
    }
}
