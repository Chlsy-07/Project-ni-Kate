using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using ProjectNiKate.Player;

namespace ProjectNiKate.Editor
{
    public class FixPlayerSetupTool
    {
        [MenuItem("Tools/Fix Player Setup")]
        public static void FixAll()
        {
            FixAnimatorController();
            FixSceneLayers();
            FixPrefabMask();
            
            Debug.Log("[FixPlayerSetup] Fixes complete! You can now test in Play Mode.");
        }

        private static void FixAnimatorController()
        {
            string path = "Assets/Animations/Player/Player.controller";
            
            // Delete the old broken controller if it exists
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            // Create a fresh controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            
            // Add parameters
            controller.AddParameter("speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("velocityY", AnimatorControllerParameterType.Float);
            controller.AddParameter("jumpTrigger", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            // Load clips
            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/player_idle.anim");
            var runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/player_run.anim");
            var jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/player_jump.anim");

            // Add states (this automatically adds the state object to the state machine asset under the hood)
            var idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            var runState = rootStateMachine.AddState("Run");
            runState.motion = runClip;

            var jumpState = rootStateMachine.AddState("Jump");
            jumpState.motion = jumpClip;

            // Set default state
            rootStateMachine.defaultState = idleState;

            // Wire transitions
            
            // Idle -> Run
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0;
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "speed");

            // Run -> Idle
            var runToIdle = runState.AddTransition(idleState);
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0;
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");

            // Any State -> Jump
            var anyToJump = rootStateMachine.AddAnyStateTransition(jumpState);
            anyToJump.hasExitTime = false;
            anyToJump.duration = 0;
            anyToJump.AddCondition(AnimatorConditionMode.If, 0, "jumpTrigger");

            // Jump -> Idle
            var jumpToIdle = jumpState.AddTransition(idleState);
            jumpToIdle.hasExitTime = false;
            jumpToIdle.duration = 0;
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
            jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");

            // Jump -> Run
            var jumpToRun = jumpState.AddTransition(runState);
            jumpToRun.hasExitTime = false;
            jumpToRun.duration = 0;
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
            jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "speed");

            // Force save
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            Debug.Log("[FixPlayerSetup] Rebuilt Animator Controller.");
        }

        private static void FixSceneLayers()
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1)
            {
                Debug.LogError("[FixPlayerSetup] 'Ground' layer does not exist in TagManager! Add it to User Layer 6.");
                return;
            }

            int count = 0;
            // Find all objects in scene
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var go in allObjects)
            {
                if (go.name.StartsWith("tile-sheet_57"))
                {
                    go.layer = groundLayer;
                    EditorUtility.SetDirty(go);
                    count++;
                }
            }
            
            if (count > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log($"[FixPlayerSetup] Moved {count} floor tiles to the Ground layer.");
            }
        }

        private static void FixPrefabMask()
        {
            string prefabPath = "Assets/Prefabs/Player/Player.prefab";
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var prefabRoot = scope.prefabContentsRoot;
                var controller = prefabRoot.GetComponent<PlayerController>();
                if (controller != null)
                {
                    var serializedObject = new SerializedObject(controller);
                    var groundLayerProp = serializedObject.FindProperty("_groundLayer");
                    if (groundLayerProp != null)
                    {
                        groundLayerProp.intValue = LayerMask.GetMask("Ground");
                        serializedObject.ApplyModifiedProperties();
                        Debug.Log("[FixPlayerSetup] Fixed Ground Layer mask on Player prefab.");
                    }
                }
            }
        }
    }
}
