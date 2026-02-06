using System.Collections;
using System.Collections.Generic;
using Floodline.Core.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Floodline.Client.Tests
{
    /// <summary>
    /// PlayMode tests for input feel implementation.
    /// Tests the input layer using scripted fixtures to verify:
    /// - DAS/ARR (Delayed Auto-Shift / Auto-Repeat Rate) logic
    /// - Command generation accuracy
    /// - Determinism across frames
    /// </summary>
    public class InputFeelPlayModeTests
    {
        private GameObject testGameObject;
        private InputManager inputManager;
        private TextAsset fixtureAsset;

        [SetUp]
        public void SetUp()
        {
            // Create a test GameObject with InputManager
            testGameObject = new GameObject("InputManagerTest");
            inputManager = testGameObject.AddComponent<InputManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(testGameObject);
        }

        /// <summary>
        /// Test: Player holds right arrow for an extended period.
        /// Expects DAS of 10 ticks, then ARR of 2 ticks per repeat.
        /// </summary>
        [UnityTest]
        public IEnumerator MoveRight_AppliesDASAndARR()
        {
            // Load the scripted input fixture
            var fixture = Resources.Load<TextAsset>("Tests/Fixtures/input_move_right_das");
            Assert.IsNotNull(fixture, "Test fixture 'input_move_right_das.json' not found");

            var scriptedSource = testGameObject.AddComponent<ScriptedInputSource>();
            scriptedSource.LoadFromJson(fixture.text);
            inputManager.InputSource = scriptedSource;

            // Simulate 13 ticks with continuous right input
            List<InputCommand> commands = new();
            for (int i = 0; i < 13; i++)
            {
                var cmd = inputManager.SampleAndGenerateCommand();
                commands.Add(cmd);
                inputManager.buffer.TickCooldowns();  // Note: This would need to be exposed or I'll adjust approach
                yield return null;  // Advance by one frame
            }

            // Expected: MoveRight on tick 0, silence 1-9 (DAS), then MoveRight at 10, 12 (ARR)
            Assert.AreEqual(InputCommand.MoveRight, commands[0], "Tick 0: Should move immediately");
            Assert.AreEqual(InputCommand.None, commands[1], "Tick 1: DAS delay");
            Assert.AreEqual(InputCommand.None, commands[9], "Tick 9: DAS delay");
            Assert.AreEqual(InputCommand.MoveRight, commands[10], "Tick 10: ARR fires");
            Assert.AreEqual(InputCommand.None, commands[11], "Tick 11: ARR gap");
            Assert.AreEqual(InputCommand.MoveRight, commands[12], "Tick 12: ARR fires");
        }

        /// <summary>
        /// Test: Player changes direction mid-movement.
        /// Expects DAS to restart when direction changes.
        /// </summary>
        [UnityTest]
        public IEnumerator MoveLeft_ChangeDirRestartsDASpeed()
        {
            // Create a custom fixture with direction change
            var rawInputSequence = new ScriptedInputSource.InputFixture
            {
                description = "Move right for 12 ticks, then switch to left",
                frames = new List<ScriptedInputSource.InputFrame>
                {
                    // Right movement for 12 ticks
                    CreateFrame(0, moveX: 1),
                    CreateFrame(1, moveX: 1),
                    CreateFrame(2, moveX: 1),
                    CreateFrame(3, moveX: 1),
                    CreateFrame(4, moveX: 1),
                    CreateFrame(5, moveX: 1),
                    CreateFrame(6, moveX: 1),
                    CreateFrame(7, moveX: 1),
                    CreateFrame(8, moveX: 1),
                    CreateFrame(9, moveX: 1),
                    CreateFrame(10, moveX: 1),
                    CreateFrame(11, moveX: 1),
                    // Switch to left at tick 12
                    CreateFrame(12, moveX: -1),
                    CreateFrame(13, moveX: -1),
                    CreateFrame(14, moveX: -1),
                }
            };

            var scriptedSource = testGameObject.AddComponent<ScriptedInputSource>();
            scriptedSource.LoadFromJson(JsonUtility.ToJson(rawInputSequence));
            inputManager.InputSource = scriptedSource;

            // Simulate ticks and collect commands
            List<InputCommand> commands = new();
            for (int i = 0; i < 15; i++)
            {
                var cmd = inputManager.SampleAndGenerateCommand();
                commands.Add(cmd);
                yield return null;
            }

            // Verify results
            Assert.AreEqual(InputCommand.MoveRight, commands[0], "Tick 0: Initial right move");
            Assert.AreEqual(InputCommand.MoveRight, commands[10], "Tick 10: ARR right");
            Assert.AreEqual(InputCommand.MoveLeft, commands[12], "Tick 12: Immediate left (direction change restarts DAS)");
        }

        /// <summary>
        /// Test: Hard drop command fires correctly
        /// </summary>
        [UnityTest]
        public IEnumerator HardDrop_FiresImmediately()
        {
            var rawInputSequence = new ScriptedInputSource.InputFixture
            {
                description = "Test hard drop",
                frames = new List<ScriptedInputSource.InputFrame>
                {
                    CreateFrame(0, hardDrop: false),
                    CreateFrame(1, hardDrop: true),
                    CreateFrame(2, hardDrop: false),
                }
            };

            var scriptedSource = testGameObject.AddComponent<ScriptedInputSource>();
            scriptedSource.LoadFromJson(JsonUtility.ToJson(rawInputSequence));
            inputManager.InputSource = scriptedSource;

            List<InputCommand> commands = new();
            for (int i = 0; i < 3; i++)
            {
                var cmd = inputManager.SampleAndGenerateCommand();
                commands.Add(cmd);
                yield return null;
            }

            Assert.AreEqual(InputCommand.None, commands[0]);
            Assert.AreEqual(InputCommand.HardDrop, commands[1]);
            Assert.AreEqual(InputCommand.None, commands[2]);
        }

        // Helper to create a test input frame
        private static ScriptedInputSource.InputFrame CreateFrame(
            int tick,
            int moveX = 0,
            int moveZ = 0,
            bool hardDrop = false,
            bool softDrop = false
        )
        {
            return new ScriptedInputSource.InputFrame
            {
                tick = tick,
                input = new RawInputState
                {
                    moveX = moveX,
                    moveZ = moveZ,
                    hardDrop = hardDrop,
                    softDrop = softDrop,
                }
            };
        }
    }
}
