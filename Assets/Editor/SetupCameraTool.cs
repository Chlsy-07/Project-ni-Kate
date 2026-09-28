using UnityEditor;
using UnityEngine;
using ProjectNiKate.Gameplay;

namespace ProjectNiKate.Editor
{
    public class SetupCameraTool
    {
        [MenuItem("Tools/Setup/Setup Camera and Background")]
        public static void SetupCameraAndBackground()
        {
            // 1. Locate Main Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = GameObject.Find("Main Camera");
                if (camObj != null) mainCam = camObj.GetComponent<Camera>();
            }

            if (mainCam == null)
            {
                Debug.LogError("[SetupCameraTool] Could not find Main Camera!");
                return;
            }

            // 2. Add or update CameraFollow
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null)
            {
                camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            }

            GameObject player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (player != null)
            {
                var serializedCam = new SerializedObject(camFollow);
                serializedCam.FindProperty("_target").objectReferenceValue = player.transform;
                serializedCam.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[SetupCameraTool] Player not found in scene. Assign the target manually.");
            }

            // 3. Fix Sprite Import Settings
            string spritePath = "Assets/Sprites/Environment/Background.jpg";
            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                bool needsReimport = false;
                if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; needsReimport = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; needsReimport = true; }
                if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; needsReimport = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; needsReimport = true; }
                
                if (needsReimport)
                {
                    importer.SaveAndReimport();
                    Debug.Log("[SetupCameraTool] Updated Background.jpg import settings.");
                }
            }
            else
            {
                Debug.LogError($"[SetupCameraTool] Could not find background sprite at {spritePath}");
                return;
            }

            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            // 4. Create or Update Background_Fixed child
            Transform bgTransform = mainCam.transform.Find("Background_Fixed");
            GameObject bgObj;
            SpriteRenderer sr;

            if (bgTransform == null)
            {
                bgObj = new GameObject("Background_Fixed");
                bgObj.transform.SetParent(mainCam.transform);
                sr = bgObj.AddComponent<SpriteRenderer>();
            }
            else
            {
                bgObj = bgTransform.gameObject;
                sr = bgObj.GetComponent<SpriteRenderer>();
                if (sr == null) sr = bgObj.AddComponent<SpriteRenderer>();
            }

            bgObj.transform.localPosition = new Vector3(0, 0, 10);
            bgObj.transform.localRotation = Quaternion.identity;
            sr.sprite = bgSprite;
            
            // Set Sorting Layer / Order
            sr.sortingLayerName = "Background";
            sr.sortingOrder = -100;

            // Compute Scale to fill camera view (Cover approach)
            float camHeight = mainCam.orthographicSize * 2f;
            float camWidth = camHeight * mainCam.aspect;

            float spriteWidth = bgSprite.bounds.size.x;
            float spriteHeight = bgSprite.bounds.size.y;

            float scaleX = camWidth / spriteWidth;
            float scaleY = camHeight / spriteHeight;
            float finalScale = Mathf.Max(scaleX, scaleY); // 'Cover' keeps aspect ratio and fills view

            bgObj.transform.localScale = new Vector3(finalScale, finalScale, 1f);

            Debug.Log($"[SetupCameraTool] Success! Camera rect: {camWidth:F2}x{camHeight:F2}, Sprite world size: {spriteWidth:F2}x{spriteHeight:F2}, Applied Scale: {finalScale:F2}");
        }
    }
}
