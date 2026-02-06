using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Floodline.Client.Editor;
#endif

namespace Floodline.Client.Editor.Tests
{
    /// <summary>
    /// Acceptance tests for the Unity MVP Constraints Pack.
    /// Verifies that all required placeholder assets exist and meet constraint requirements.
    /// </summary>
    public class ConstraintPackTests
    {
        [SetUp]
        public void Setup()
        {
#if UNITY_EDITOR
            // Ensure placeholder assets are generated before each test
            PlaceholderAssetGenerator.EnsureAllAssetsExist();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        #region Material Tests

        [Test]
        public void Assets_Exist_For_All_Materials()
        {
            string[] materialNames = new[]
            {
                "Mat_Solid_Standard",
                "Mat_Solid_Heavy",
                "Mat_Solid_Reinforced",
                "Mat_Water_Surface",
                "Mat_Ice_Crystalline",
                "Mat_Wall_Bedrock",
                "Mat_Drain_Hole",
                "Mat_Porous_Support"
            };

            foreach (var materialName in materialNames)
            {
                var material = Resources.Load<Material>($"Materials/{materialName}");
                Assert.IsNotNull(
                    material,
                    $"Required material '{materialName}' not found. Run PlaceholderAssetGenerator."
                );
            }
        }

        [Test]
        public void All_Materials_Use_Valid_Shader()
        {
            string[] materialNames = new[]
            {
                "Mat_Solid_Standard",
                "Mat_Solid_Heavy",
                "Mat_Solid_Reinforced",
                "Mat_Water_Surface",
                "Mat_Ice_Crystalline",
                "Mat_Wall_Bedrock",
                "Mat_Drain_Hole",
                "Mat_Porous_Support"
            };

            foreach (var materialName in materialNames)
            {
                var material = Resources.Load<Material>($"Materials/{materialName}");
                Assert.IsNotNull(material.shader, $"Material '{materialName}' has no shader assigned.");
            }
        }

        #endregion

        #region UI Sprite Tests

        [Test]
        public void Assets_Exist_For_All_UI_Sprites()
        {
            string[] spriteNames = new[]
            {
                "gravity-arrow",
                "heart-icon",
                "wind-icon",
                "target-icon"
            };

            foreach (var spriteName in spriteNames)
            {
                var sprite = Resources.Load<Sprite>($"UI/Sprites/{spriteName}");
                Assert.IsNotNull(
                    sprite,
                    $"Required sprite '{spriteName}' not found. Run PlaceholderAssetGenerator."
                );
            }
        }

        #endregion

        #region HUD Prefab Tests

        [Test]
        public void HUD_Canvas_Prefab_Exists()
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HUD_Canvas.prefab");
            Assert.IsNotNull(
                prefab,
                "HUD Canvas prefab not found at Assets/Prefabs/HUD_Canvas.prefab. Run PlaceholderAssetGenerator."
            );
#endif
        }

        [Test]
        public void HUD_Canvas_Has_Required_Components()
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HUD_Canvas.prefab");
            Assert.IsNotNull(prefab, "HUD Canvas prefab must exist first.");

            Assert.IsNotNull(
                prefab.GetComponent<Canvas>(),
                "HUD Canvas prefab must have a Canvas component."
            );

            Assert.IsNotNull(
                prefab.GetComponent<CanvasScaler>(),
                "HUD Canvas prefab must have a CanvasScaler component."
            );

            Assert.IsNotNull(
                prefab.GetComponent<GraphicRaycaster>(),
                "HUD Canvas prefab must have a GraphicRaycaster component."
            );
#endif
        }

        #endregion

        #region Folder Structure Tests

        [Test]
        public void Required_Folders_Exist()
        {
            string[] folderPaths = new[]
            {
                "Assets/Materials",
                "Assets/UI/Sprites",
                "Assets/Audio/SFX",
                "Assets/Audio/Music",
                "Assets/Prefabs"
            };

#if UNITY_EDITOR
            foreach (var folderPath in folderPaths)
            {
                Assert.IsTrue(
                    AssetDatabase.IsValidFolder(folderPath),
                    $"Required folder '{folderPath}' does not exist."
                );
            }
#endif
        }

        #endregion

        #region Documentation Tests

        [Test]
        public void Assets_Documentation_Exists()
        {
            // Check that the constraints document exists in the repository root/docs
            var constraintsPath = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).Parent.FullName,
                "docs",
                "Unity_MVP_Constraints.md"
            );

            Assert.IsTrue(
                System.IO.File.Exists(constraintsPath),
                $"Constraints document not found at {constraintsPath}"
            );
        }

        #endregion
    }
}
