using UnityEditor;
using UnityEngine;
public class ForceAssignController {
    [MenuItem("Tools/Force Assign Controller")]
    public static void ForceAssign() {
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Player/Player.controller");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
        var animator = prefab.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        var playerInScene = GameObject.Find("Player");
        if (playerInScene != null) {
            var sceneAnimator = playerInScene.GetComponent<Animator>();
            if (sceneAnimator != null) {
                sceneAnimator.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(sceneAnimator);
            }
        }
        Debug.Log("Controller force assigned!");
    }
}
