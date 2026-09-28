using UnityEditor;
using UnityEngine;

namespace ProjectNiKate.Editor
{
    public static class ProjectSetupTool
    {
        [MenuItem("Tools/Player/0. Setup Project Settings")]
        public static void SetupSettings()
        {
            SetupInputHandler();
            SetupLayers();
            Debug.Log("[ProjectSetupTool] Project settings configured successfully.");
        }

        private static void SetupInputHandler()
        {
            var projectSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (projectSettings == null || projectSettings.Length == 0) return;
            
            SerializedObject so = new SerializedObject(projectSettings[0]);
            SerializedProperty activeInputHandler = so.FindProperty("activeInputHandler");
            
            if (activeInputHandler != null && activeInputHandler.intValue != 2)
            {
                activeInputHandler.intValue = 2; // Both
                so.ApplyModifiedProperties();
                Debug.Log("[ProjectSetupTool] activeInputHandler set to Both (2). Editor restart may be required to take full effect.");
            }
        }

        private static void SetupLayers()
        {
            var tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManager == null || tagManager.Length == 0) return;
            
            SerializedObject so = new SerializedObject(tagManager[0]);
            SerializedProperty layers = so.FindProperty("layers");

            bool found = false;
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerProp = layers.GetArrayElementAtIndex(i);
                if (layerProp.stringValue == "Ground")
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                for (int i = 8; i < layers.arraySize; i++)
                {
                    SerializedProperty layerProp = layers.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(layerProp.stringValue))
                    {
                        layerProp.stringValue = "Ground";
                        so.ApplyModifiedProperties();
                        Debug.Log($"[ProjectSetupTool] Added 'Ground' to Layer {i}.");
                        break;
                    }
                }
            }
        }
    }
}
