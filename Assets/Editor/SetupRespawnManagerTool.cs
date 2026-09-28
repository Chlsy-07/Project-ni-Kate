using UnityEditor;
using UnityEngine;
using ProjectNiKate.Gameplay;

namespace ProjectNiKate.Editor
{
    public class SetupRespawnManagerTool
    {
        [MenuItem("Tools/Environment/Create Respawn Manager")]
        public static void CreateRespawnManager()
        {
            // Check if one already exists
            var existing = Object.FindAnyObjectByType<RespawnManager>();
            if (existing != null)
            {
                Debug.LogWarning("[SetupRespawnManager] A RespawnManager already exists in the scene!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            // Create GameObject
            GameObject root = new GameObject("RespawnManager");
            
            // Add Script
            root.AddComponent<RespawnManager>();

            // Create a default spawn point slightly above 0,0
            GameObject spawnPoint = new GameObject("Default_SpawnPoint");
            spawnPoint.transform.position = new Vector3(0, 2, 0);
            
            // Assign the spawn point to the manager
            var serializedObject = new SerializedObject(root.GetComponent<RespawnManager>());
            serializedObject.FindProperty("_spawnPoint").objectReferenceValue = spawnPoint.transform;
            serializedObject.ApplyModifiedProperties();

            // Select it in editor
            Selection.activeGameObject = root;

            Debug.Log("[SetupRespawnManager] Successfully created RespawnManager in the scene!");
        }
    }
}
