using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNiKate.Editor
{
    public static class BuildCharacterAnimationsTool
    {
        private const string SpritesPath = "Assets/Sprites/Character";
        private const string AnimOutputPath = "Assets/Animations/Player";
        private const int FrameRate = 6;

        [MenuItem("Tools/Player/1. Build Animations")]
        public static void BuildAll()
        {
            // 1. Process Sprites
            ProcessSprites();

            // 2. Build Clips
            AnimationClip idleClip = BuildClip("idle", "player_idle.anim", true);
            AnimationClip runClip = BuildClip("run", "player_run.anim", true);
            AnimationClip jumpClip = BuildClip("jump", "player_jump.anim", false);

            if (idleClip == null || runClip == null || jumpClip == null)
            {
                Debug.LogError("[BuildCharacterAnimationsTool] Missing clips. Controller generation aborted.");
                return;
            }

            // 3. Build Controller
            BuildController(idleClip, runClip, jumpClip);

            Debug.Log("[BuildCharacterAnimationsTool] Animations and Controller built successfully.");
        }

        private static void ProcessSprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;

                    if (importer.filterMode != FilterMode.Point)
                    {
                        importer.filterMode = FilterMode.Point;
                        changed = true;
                    }

                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }

                    if (importer.spritePivot != new Vector2(0.5f, 0f) || importer.spriteImportMode != SpriteImportMode.Multiple)
                    {
                        // To set pivot cleanly, usually we ensure it's Single or we adjust the rects if it's Multiple.
                        // Currently the images are SpriteMode Multiple with a single slice.
                        // We will set the alignment to BottomCenter.
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.spritePivot = new Vector2(0.5f, 0f);
                        TextureImporterSettings settings = new TextureImporterSettings();
                        importer.ReadTextureSettings(settings);
                        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                        importer.SetTextureSettings(settings);
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        private static AnimationClip BuildClip(string prefix, string outputName, bool isLooping)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefix} t:Sprite", new[] { SpritesPath });
            List<Sprite> sprites = new List<Sprite>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path).ToLower().StartsWith(prefix))
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null) sprites.Add(sprite);
                }
            }

            // Sort alphabetically to ensure idle1, idle2... order
            sprites = sprites.OrderBy(s => s.name).ToList();

            if (sprites.Count == 0)
            {
                Debug.LogError($"[BuildCharacterAnimationsTool] No sprites found with prefix: {prefix}");
                return null;
            }

            AnimationClip clip = new AnimationClip();
            clip.frameRate = FrameRate;

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count + 1];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / (float)FrameRate,
                    value = sprites[i]
                };
            }
            // Add a final keyframe to hold the last sprite duration properly
            keyframes[sprites.Count] = new ObjectReferenceKeyframe
            {
                time = sprites.Count / (float)FrameRate,
                value = sprites[sprites.Count - 1]
            };

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            if (isLooping)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            string fullPath = $"{AnimOutputPath}/{outputName}";
            
            // Delete existing
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(fullPath) != null)
            {
                AssetDatabase.DeleteAsset(fullPath);
            }

            AssetDatabase.CreateAsset(clip, fullPath);
            return clip;
        }

        private static void BuildController(AnimationClip idle, AnimationClip run, AnimationClip jump)
        {
            string path = $"{AnimOutputPath}/Player.controller";
            
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Add Parameters
            controller.AddParameter("speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("velocityY", AnimatorControllerParameterType.Float);
            controller.AddParameter("jumpTrigger", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // Add States
            AnimatorState stateIdle = sm.AddState("Idle");
            stateIdle.motion = idle;

            AnimatorState stateRun = sm.AddState("Run");
            stateRun.motion = run;

            AnimatorState stateJump = sm.AddState("Jump");
            stateJump.motion = jump;

            sm.defaultState = stateIdle;

            // Transitions
            // Idle -> Run
            var idleToRun = stateIdle.AddTransition(stateRun);
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0f;
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "speed");

            // Run -> Idle
            var runToIdle = stateRun.AddTransition(stateIdle);
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0f;
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");

            // Any -> Jump
            var anyToJump = sm.AddAnyStateTransition(stateJump);
            anyToJump.hasExitTime = false;
            anyToJump.duration = 0f;
            anyToJump.AddCondition(AnimatorConditionMode.If, 0f, "jumpTrigger");

            // Jump -> Idle
            var jumpToIdle = stateJump.AddTransition(stateIdle);
            jumpToIdle.hasExitTime = false;
            jumpToIdle.duration = 0f;
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "isGrounded");
            jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "speed");

            // Jump -> Run
            var jumpToRun = stateJump.AddTransition(stateRun);
            jumpToRun.hasExitTime = false;
            jumpToRun.duration = 0f;
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "isGrounded");
            jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "speed");

            AssetDatabase.SaveAssets();
        }
    }
}
