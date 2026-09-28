using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ProjectNiKate.Gameplay;

namespace ProjectNiKate.Editor
{
    public static class SetupEnvironmentTool
    {
        // ── Asset paths ───────────────────────────────────────────────────────
        // NOTE: Slices are named "tile-sheet_N" (hyphen, not underscore).
        private const string TileSheetPath = "Assets/Sprites/Environment/tile-sheet.png";
        private const string DirtPath      = "Assets/Sprites/Environment/dirt.png";
        private const string GrassPath     = "Assets/Sprites/Environment/grass.png";
        private const string RockPath      = "Assets/Sprites/Environment/rock.png";

        // ── Prefab output folders (match old project's names) ─────────────────
        private const string SolidFolder    = "Assets/Prefabs/Environment/Solid";
        private const string HazardFolder   = "Assets/Prefabs/Environment/Hazards";
        private const string PlatformFolder = "Assets/Prefabs/Environment/Platforms";

        // ─────────────────────────────────────────────────────────────────────
        [MenuItem("Tools/Environment/Build Environment Prefabs")]
        public static void BuildAll()
        {
            EnsureFolderExists(SolidFolder);
            EnsureFolderExists(HazardFolder);
            EnsureFolderExists(PlatformFolder);

            int solidCount    = BuildSolids();
            int hazardCount   = BuildHazards();
            int platformCount = BuildPlatforms();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SetupEnvironmentTool] Created {solidCount} solid prefabs");
            Debug.Log($"[SetupEnvironmentTool] Created {hazardCount} hazard prefabs");
            Debug.Log($"[SetupEnvironmentTool] Created {platformCount} platform prefabs");
            Debug.Log($"[SetupEnvironmentTool] Total: {solidCount + hazardCount + platformCount} prefabs");
        }

        // ── Solid tiles ───────────────────────────────────────────────────────
        private static int BuildSolids()
        {
            int count = 0;

            // ── Individual sprites (dirt, grass, rock) ──
            count += BuildSingleSprite(DirtPath,  SolidFolder, isHazard: false);
            count += BuildSingleSprite(GrassPath, SolidFolder, isHazard: false);
            count += BuildSingleSprite(RockPath,  SolidFolder, isHazard: false);

            // ── Slices from tile-sheet.png ──
            Sprite[] allSlices = LoadAllSprites(TileSheetPath);
            if (allSlices == null || allSlices.Length == 0)
            {
                Debug.LogWarning($"[SetupEnvironmentTool] Could not load slices from {TileSheetPath}. Skipping sheet solids.");
                return count;
            }

            // Solid slice indices: 2-7, 9-13, 45, 57, 58
            var solidIndices = new HashSet<int>();
            for (int i = 2;  i <= 7;  i++) solidIndices.Add(i);
            for (int i = 9;  i <= 13; i++) solidIndices.Add(i);
            solidIndices.Add(45);
            solidIndices.Add(57);
            solidIndices.Add(58);

            count += BuildSliceSubset(allSlices, "tile-sheet", solidIndices, SolidFolder, isHazard: false);

            return count;
        }

        // ── Hazard tiles ──────────────────────────────────────────────────────
        private static int BuildHazards()
        {
            Sprite[] allSlices = LoadAllSprites(TileSheetPath);
            if (allSlices == null || allSlices.Length == 0)
            {
                Debug.LogWarning($"[SetupEnvironmentTool] Could not load slices from {TileSheetPath}. Skipping hazards.");
                return 0;
            }

            // Hazard slice indices: 15-17, 19-24
            var hazardIndices = new HashSet<int>();
            for (int i = 15; i <= 17; i++) hazardIndices.Add(i);
            for (int i = 19; i <= 24; i++) hazardIndices.Add(i);

            return BuildSliceSubset(allSlices, "tile-sheet", hazardIndices, HazardFolder, isHazard: true);
        }

        // ── Platform tiles ────────────────────────────────────────────────────
        private static int BuildPlatforms()
        {
            Sprite[] allSlices = LoadAllSprites(TileSheetPath);
            if (allSlices == null || allSlices.Length == 0)
            {
                Debug.LogWarning($"[SetupEnvironmentTool] Could not load slices from {TileSheetPath}. Skipping platforms.");
                return 0;
            }

            // Platform slice indices: 61, 62
            var platformIndices = new HashSet<int> { 61, 62 };

            return BuildSliceSubset(allSlices, "tile-sheet", platformIndices, PlatformFolder, isHazard: false);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Builds prefabs for the subset of sprites whose name matches
        /// "{prefix}_{index}" for each index in the provided set.
        /// </summary>
        private static int BuildSliceSubset(
            Sprite[]      allSlices,
            string        prefix,
            HashSet<int>  indices,
            string        outFolder,
            bool          isHazard)
        {
            int count = 0;
            foreach (Sprite sprite in allSlices)
            {
                // Expected name format: "tile-sheet_57"
                string suffix = sprite.name.Replace(prefix + "_", "");
                if (int.TryParse(suffix, out int idx) && indices.Contains(idx))
                {
                    CreatePrefab(outFolder, sprite, isHazard);
                    count++;
                }
            }

            // Warn about any requested indices not found in the sheet
            var found = new HashSet<int>();
            foreach (Sprite sprite in allSlices)
            {
                string suffix = sprite.name.Replace(prefix + "_", "");
                if (int.TryParse(suffix, out int idx) && indices.Contains(idx))
                    found.Add(idx);
            }

            foreach (int idx in indices)
            {
                if (!found.Contains(idx))
                {
                    Debug.LogWarning(
                        $"[SetupEnvironmentTool] Sprite '{prefix}_{idx}' not found in {TileSheetPath}. " +
                        "Skipping.");
                }
            }

            return count;
        }

        /// <summary>
        /// Loads the first (or only) sprite from a single-sprite PNG and creates a prefab.
        /// Returns 1 on success, 0 on failure.
        /// </summary>
        private static int BuildSingleSprite(string assetPath, string outFolder, bool isHazard)
        {
            Sprite[] sprites = LoadAllSprites(assetPath);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning(
                    $"[SetupEnvironmentTool] Could not load sprite at '{assetPath}'. Skipping.");
                return 0;
            }

            CreatePrefab(outFolder, sprites[0], isHazard);
            return 1;
        }

        /// <summary>
        /// Creates or overwrites a prefab at {outFolder}/{sprite.name}.prefab.
        ///
        /// Solid prefabs:  single BoxCollider2D at 95% height (avoids transparent top edge).
        /// Hazard prefabs: dual collider — a solid 70% block for physics + a trigger 85%
        ///                 for the kill zone, mirroring the old project's pattern exactly.
        /// </summary>
        private static void CreatePrefab(string outFolder, Sprite sprite, bool isHazard)
        {
            string path = $"{outFolder}/{sprite.name}.prefab";

            // Reuse existing prefab if present (idempotent)
            bool overwriting = File.Exists(path);

            GameObject root = new GameObject(sprite.name);

            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;

            var col = root.AddComponent<BoxCollider2D>();
            // BoxCollider2D auto-sizes to the sprite bounds when a SpriteRenderer is present.
            Vector2 origSize   = sprite.bounds.size;
            Vector2 origOffset = Vector2.zero; // pivot is typically center

            if (isHazard)
            {
                // Hazard: Only ONE collider.
                // Needs to be a trigger, shrunk to the "inside layer" (80%) so it's forgiving.
                // No solid collider, so the player's body isn't physically blocked on the outside.
                col.size = new Vector2(origSize.x * 0.80f, origSize.y * 0.80f);
                col.offset = origOffset;
                col.isTrigger = true;

                root.AddComponent<KillZone>();
            }
            else
            {
                // Solid block: shave 5% off height and shift down slightly to avoid
                // the player hovering on transparent top-edge pixels.
                col.size   = new Vector2(origSize.x, origSize.y * 0.95f);
                col.offset = new Vector2(origOffset.x, origOffset.y - origSize.y * 0.025f);
                col.isTrigger = false;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            if (overwriting)
                Debug.Log($"[SetupEnvironmentTool] Updated  {path}");
        }

        private static Sprite[] LoadAllSprites(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                                .OfType<Sprite>()
                                .ToArray();
        }

        private static void EnsureFolderExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent    = Path.GetDirectoryName(path).Replace("\\", "/");
            string newFolder = Path.GetFileName(path);
            EnsureFolderExists(parent);
            AssetDatabase.CreateFolder(parent, newFolder);
        }
    }
}
