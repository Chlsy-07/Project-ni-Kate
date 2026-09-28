using UnityEditor;
using UnityEngine;
using ProjectNiKate.Gameplay.Environment;

namespace ProjectNiKate.Editor
{
    public class SetupCheckpointTool
    {
        [MenuItem("Tools/Environment/Build Checkpoint Prefab")]
        public static void BuildCheckpoint()
        {
            string spritePath = "Assets/Sprites/Environment/tile-sheet.png";
            string folderPath = "Assets/Prefabs/Environment/Interactables";
            string prefabPath = $"{folderPath}/Checkpoint_Flag.prefab";

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = System.IO.Path.GetDirectoryName(folderPath).Replace("\\", "/");
                string newFolder = System.IO.Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parent, newFolder);
            }

            // Find tile-sheet_51 sprite
            Sprite flagSprite = null;
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s && s.name == "tile-sheet_51")
                {
                    flagSprite = s;
                    break;
                }
            }

            if (flagSprite == null)
            {
                Debug.LogError("[SetupCheckpointTool] Could not find 'tile-sheet_51' sprite inside tile-sheet.png.");
                return;
            }

            // Create GameObject
            GameObject root = new GameObject("Checkpoint_Flag");

            // Add SpriteRenderer
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = flagSprite;

            // Add Collider
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(flagSprite.bounds.size.x, flagSprite.bounds.size.y);
            col.offset = Vector2.zero; // default centered

            // Add Checkpoint Script
            root.AddComponent<Checkpoint>();

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[SetupCheckpointTool] Successfully created Checkpoint prefab at {prefabPath}");
        }
    }
}
