using System;
using UnityEngine;
using Floodline.Core;
using Floodline.Core.Levels;

namespace Floodline.Client
{
    /// <summary>
    /// Minimal bootstrap loader for a Floodline level.
    /// Loads a level JSON, creates a simulation, and runs it headlessly.
    /// For M5, this is the proof that Unity can import and execute Core without modifications.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        [SerializeField]
        private string levelPath = "levels/minimal_level.json";

        [SerializeField]
        private uint seed = 0;

        private Simulation simulation;
        private int ticksUntilDone = 0;

        private void Start()
        {
            LoadAndInitializeSimulation();
        }

        private void Update()
        {
            if (simulation != null && simulation.Status == SimulationStatus.Playing)
            {
                // Run one tick per frame (this is a headless runner; no frame-rate constraint for determinism)
                var inputs = new InputCommand[1];
                inputs[0] = InputCommand.None;
                simulation.Tick(inputs);

                if (simulation.Status != SimulationStatus.Playing)
                {
                    LogSimulationResult();
                }
            }
        }

        private void LoadAndInitializeSimulation()
        {
            try
            {
                // Construct the full path to the level file relative to the project root
                string projectRoot = System.IO.Path.Combine(Application.dataPath, "..");
                string fullLevelPath = System.IO.Path.Combine(projectRoot, levelPath);

                // Load the level
                if (!System.IO.File.Exists(fullLevelPath))
                {
                    Debug.LogError($"Level file not found: {fullLevelPath}");
                    return;
                }

                string levelJson = System.IO.File.ReadAllText(fullLevelPath);
                var level = LevelLoader.Load(levelJson, levelPath);

                // Create a deterministic PRNG seeded from the command-line arg or default
                var prng = new Pcg32(seed);

                // Create the simulation
                simulation = new Simulation(level, prng);
                Debug.Log($"Simulation initialized: level={level.Meta.Id}, seed={seed}, status={simulation.Status}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load simulation: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LogSimulationResult()
        {
            Debug.Log($"Simulation finished: status={simulation.Status}");
            if (!string.IsNullOrEmpty(simulation.Status.ToString()))
            {
                var state = simulation.State;
                Debug.Log($"  Final objective status: {(state.Objective?.Status.ToString() ?? "none")}");
            }
        }
    }
}
