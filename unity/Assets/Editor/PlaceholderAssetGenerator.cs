using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Floodline.Client.Editor
{
    /// <summary>
    /// Generates placeholder assets (materials, sprites, audio) required by the MVP constraints pack.
    /// Called automatically at Editor startup or manually via Assets > Floodline > Generate Placeholder Assets.
    /// </summary>
    public static class PlaceholderAssetGenerator
    {
        private const string MaterialFolder = "Assets/Materials";
        private const string SpriteFolder = "Assets/UI/Sprites";
        private const string SfxFolder = "Assets/Audio/SFX";
        private const string MusicFolder = "Assets/Audio/Music";
        private const string PrefabFolder = "Assets/Prefabs";

        private static readonly string[] MaterialNames = new[]
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

        private static readonly string[] SpriteNames = new[]
        {
            "gravity-arrow",
            "heart-icon",
            "wind-icon",
            "target-icon"
        };

        private static readonly string[] AudioClipNames = new[]
        {
            "sfx_lock",
            "sfx_drop",
            "sfx_rotate",
            "sfx_collapse",
            "sfx_water_flow",
            "sfx_drain_tick",
            "sfx_freeze",
            "sfx_thaw",
            "sfx_wind_gust",
            "mus_calm",
            "mus_tension",
            "mus_danger"
        };

#if UNITY_EDITOR
        [MenuItem("Assets/Floodline/Generate Placeholder Assets")]
        public static void GenerateAllAssets()
        {
            Debug.Log("Generating Floodline placeholder assets...");
            
            EnsureAllAssetsExist();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Placeholder asset generation complete.");
        }

        public static void EnsureAllAssetsExist()
        {
            EnsureFoldersExist();
            EnsureMaterials();
            EnsureSprites();
            // Audio generation is complex; we skip for now and rely on EditorImport
            EnsureHUDCanvasPrefab();
        }

        private static void EnsureFoldersExist()
        {
            EnsureFolder(MaterialFolder);
            EnsureFolder(SpriteFolder);
            EnsureFolder(SfxFolder);
            EnsureFolder(MusicFolder);
            EnsureFolder(PrefabFolder);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = "";
            
            foreach (string part in parts)
            {
                current = string.IsNullOrEmpty(current) ? part : current + "/" + part;
                
                if (!AssetDatabase.IsValidFolder(current))
                {
                    string parentPath = Path.GetDirectoryName(current).Replace('\\', '/');
                    string folderName = Path.GetFileName(current);
                    AssetDatabase.CreateFolder(parentPath, folderName);
                    Debug.Log($"Created folder: {current}");
                }
            }
        }

        private static void EnsureMaterials()
        {
            foreach (var materialName in MaterialNames)
            {
                string path = $"{MaterialFolder}/{materialName}.mat";
                
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                {
                    Debug.Log($"Material exists: {path}");
                    continue;
                }

                Material material = new Material(Shader.Find("Standard"));
                material.name = materialName;
                
                // Set colors based on material type
                if (materialName.Contains("Water"))
                {
                    material.color = new Color(0.2f, 0.8f, 1f, 0.5f);
                }
                else if (materialName.Contains("Ice"))
                {
                    material.color = new Color(0.5f, 1f, 1f, 0.8f);
                }
                else if (materialName.Contains("Heavy"))
                {
                    material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                }
                else if (materialName.Contains("Reinforced"))
                {
                    material.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                }
                else if (materialName.Contains("Drain") || materialName.Contains("Bedrock"))
                {
                    material.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                }
                else
                {
                    material.color = Color.white;
                }

                AssetDatabase.CreateAsset(material, path);
                Debug.Log($"Created material: {path}");
            }
        }

        private static void EnsureSprites()
        {
            foreach (var spriteName in SpriteNames)
            {
                string path = $"{SpriteFolder}/{spriteName}.png";
                
                if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
                {
                    Debug.Log($"Sprite exists: {path}");
                    continue;
                }

                // Create a minimal 64x64 white PNG
                Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                Color[] colors = new Color[64 * 64];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                texture.SetPixels(colors);
                texture.Apply();

                byte[] pngData = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);

                File.WriteAllBytes(Path.Combine(Application.dataPath, "UI/Sprites", $"{spriteName}.png"), pngData);
                Debug.Log($"Created sprite texture: {path}");
            }
        }

        private static void EnsureHUDCanvasPrefab()
        {
            string path = $"{PrefabFolder}/HUD_Canvas.prefab";
            
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log($"HUD Canvas prefab exists: {path}");
                return;
            }

            // Create minimal Canvas GameObject
            GameObject canvasGO = new GameObject("HUD_Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create a basic prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(canvasGO, path);
            Object.DestroyImmediate(canvasGO);
            
            Debug.Log($"Created HUD Canvas prefab: {path}");
        }
#endif
    }

    /// <summary>
    /// Initializes placeholder assets when the Editor starts.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class PlaceholderAssetLoader
    {
        static PlaceholderAssetLoader()
        {
            // Check if we should generate on startup (optional; can be disabled)
            if (EditorPrefs.GetBool("Floodline.GeneratePlaceholdersOnStartup", true))
            {
                EditorApplication.delayCall += () => 
                {
                    // Only generate if in edit mode and not playing
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        PlaceholderAssetGenerator.EnsureAllAssetsExist();
                    }
                };
            }
        }
    }
#endif
}
