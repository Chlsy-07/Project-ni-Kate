using UnityEditor;
using UnityEngine;
using ProjectNiKate.Gameplay;
using System.Collections.Generic;

namespace ProjectNiKate.Editor
{
    public class FixHazardsTool
    {
        [MenuItem("Tools/Environment/Fix Hazard Colliders")]
        public static void FixHazards()
        {
            string[] guids = AssetDatabase.FindAssets("tile-sheet_ t:Prefab", new[] { "Assets/Prefabs/Environment/Hazards" });
            
            int fixedCount = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var root = scope.prefabContentsRoot;
                    
                    if (root.GetComponent<KillZone>() != null)
                    {
                        var colliders = root.GetComponents<BoxCollider2D>();
                        
                        // We want exactly ONE collider: a trigger that covers the "inside" (e.g. 80%)
                        // so it's forgiving. We remove the solid physical collider completely!
                        
                        // We must keep exactly ONE collider so KillZone's RequireComponent doesn't fail.
                        var firstCol = colliders[0];
                        
                        // Delete any extra colliders
                        for (int i = 1; i < colliders.Length; i++)
                        {
                            Object.DestroyImmediate(colliders[i], true);
                        }
                        
                        var sr = root.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            firstCol.isTrigger = true;
                            // Make it cover the inside layer (80% of the sprite)
                            firstCol.size = new Vector2(sr.sprite.bounds.size.x * 0.80f, sr.sprite.bounds.size.y * 0.80f);
                            firstCol.offset = Vector2.zero;
                        }
                        
                        fixedCount++;
                    }
                }
            }
            
            Debug.Log($"[FixHazards] Removed solid outer colliders and shrunk the trigger to the inside layer for {fixedCount} hazard prefabs!");
        }
    }
}
