using System.Collections.Generic;
using UnityEngine;

namespace Floodline.Client
{
    /// <summary>
    /// Scripted input source for testing.
    /// Reads a sequence of input frames from a JSON fixture and replays them deterministically.
    /// </summary>
    public class ScriptedInputSource : InputSource
    {
        /// <summary>
        /// Serializable frame from JSON fixture
        /// </summary>
        [System.Serializable]
        public class InputFrame
        {
            public int tick;  // Absolute tick number
            public RawInputState input = new RawInputState();
        }

        /// <summary>
        /// Serializable fixture container
        /// </summary>
        [System.Serializable]
        public class InputFixture
        {
            public string description;
            public List<InputFrame> frames = new List<InputFrame>();
        }

        private InputFixture currentFixture;
        private int currentFrameIndex = 0;

        /// <summary>
        /// Load fixture from JSON string
        /// </summary>
        public void LoadFromJson(string json)
        {
            currentFixture = JsonUtility.FromJson<InputFixture>(json);
            currentFrameIndex = 0;
        }

        /// <summary>
        /// Load fixture from a TextAsset
        /// </summary>
        public void LoadFromAsset(TextAsset asset)
        {
            LoadFromJson(asset.text);
        }

        /// <summary>
        /// Get the next input frame that matches the current tick
        /// </summary>
        public override RawInputState SampleInput()
        {
            if (currentFixture == null || currentFixture.frames.Count == 0)
                return new RawInputState();  // No input

            // Check if we have a frame at the current index
            if (currentFrameIndex < currentFixture.frames.Count)
            {
                var frame = currentFixture.frames[currentFrameIndex];
                // In a real scenario, we'd track current tick and match frames
                // For now, we just advance sequentially
                var result = frame.input;
                currentFrameIndex++;
                return result;
            }

            return new RawInputState();  // No more frames
        }

        /// <summary>
        /// Reset to beginning of fixture
        /// </summary>
        public override void ResetInput()
        {
            currentFrameIndex = 0;
        }
    }
}
