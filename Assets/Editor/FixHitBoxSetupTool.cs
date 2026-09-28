using UnityEditor;
using UnityEngine;
using ProjectNiKate.Player;

namespace ProjectNiKate.Editor
{
    public class FixHitBoxSetupTool
    {
        [MenuItem("Tools/Fix HitBox Setup")]
        public static void FixHitBox()
        {
            string prefabPath = "Assets/Prefabs/Player/Player.prefab";
            
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var prefabRoot = scope.prefabContentsRoot;

                // Check if HitBox already exists
                Transform existingHitBox = prefabRoot.transform.Find("HitBox");
                if (existingHitBox != null)
                {
                    Debug.Log("[FixHitBoxSetup] HitBox already exists on Player prefab. Destroying to rebuild...");
                    Object.DestroyImmediate(existingHitBox.gameObject);
                }

                // Create HitBox child
                GameObject hitBoxObj = new GameObject("HitBox");
                hitBoxObj.transform.SetParent(prefabRoot.transform, false);

                // Set Layer to Ignore Raycast (Layer 2)
                hitBoxObj.layer = LayerMask.NameToLayer("Ignore Raycast");

                // Add BoxCollider2D (IsTrigger = true) FIRST to satisfy RequireComponent
                var boxCol = hitBoxObj.AddComponent<BoxCollider2D>();
                boxCol.isTrigger = true;
                
                // Set size to cover body/head, above feet.
                // Assuming root sprite size is roughly 1.35x1.9, we make this slightly smaller.
                boxCol.size = new Vector2(0.6f, 1.1f);
                boxCol.offset = new Vector2(0f, 0.75f);

                // Add PlayerHitBox component AFTER collider exists
                hitBoxObj.AddComponent<PlayerHitBox>();

                Debug.Log("[FixHitBoxSetup] Successfully added HitBox child to Player prefab!");
            }
        }
    }
}
