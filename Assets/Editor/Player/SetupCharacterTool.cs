using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ProjectNiKate.Player;

namespace ProjectNiKate.Editor
{
    public static class SetupCharacterTool
    {
        [MenuItem("Tools/Player/2. Setup Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            string prefabPath = "Assets/Prefabs/Player/Player.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                if (!EditorUtility.DisplayDialog("Overwrite Prefab?", "A Player prefab already exists. Overwrite?", "Yes", "No"))
                {
                    return;
                }
            }

            GameObject go = new GameObject("Player");
            go.tag = "Player";

            // Add Components
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            Animator anim = go.AddComponent<Animator>();
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            PlayerController controller = go.AddComponent<PlayerController>();
            PlayerAnimatorDriver driver = go.AddComponent<PlayerAnimatorDriver>();

            // Setup Components
            // Animator
            AnimatorController controllerAsset = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Player/Player.controller");
            if (controllerAsset != null)
            {
                anim.runtimeAnimatorController = controllerAsset;
            }
            else
            {
                Debug.LogWarning("[SetupCharacterTool] Could not find Player.controller. Did you run Build Animations tool?");
            }

            // SpriteRenderer (set default sprite so it's visible in editor)
            Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Character/idle1-removebg-preview.png");
            if (defaultSprite != null)
            {
                sr.sprite = defaultSprite;
            }

            // Rigidbody2D
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;
            rb.gravityScale = 1f; // Gravity manipulated by fall multiplier

            // BoxCollider2D
            if (defaultSprite != null)
            {
                // Set size based on bounds
                col.size = defaultSprite.bounds.size;
                // Offset vertically so Bottom-Center pivot works well
                col.offset = new Vector2(0f, col.size.y / 2f);
            }
            else
            {
                col.size = new Vector2(1f, 2f);
                col.offset = new Vector2(0f, 1f);
            }

            // Set LayerMask for ground
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty groundLayerProp = so.FindProperty("_groundLayer");
            if (groundLayerProp != null)
            {
                groundLayerProp.intValue = LayerMask.GetMask("Ground");
                so.ApplyModifiedProperties();
            }

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            GameObject.DestroyImmediate(go);

            Debug.Log($"[SetupCharacterTool] Player prefab created successfully at {prefabPath}");
        }
    }
}
