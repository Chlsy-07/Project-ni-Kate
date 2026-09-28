using UnityEditor;
using UnityEngine;

namespace ProjectNiKate.Editor
{
    public class SetupBackgroundMusicTool
    {
        [MenuItem("Tools/Setup/Set Background Music")]
        public static void SetBackgroundMusic()
        {
            // 1. Refresh AssetDatabase to ensure the newly moved mp3 is visible
            AssetDatabase.Refresh();

            // 2. Load the audio clip
            string clipPath = "Assets/Audio/Music/AdhesiveWombat_NightShade.mp3";
            AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);

            if (musicClip == null)
            {
                Debug.LogError($"[SetupBackgroundMusic] Could not find audio clip at {clipPath}");
                return;
            }

            // 3. Find or Create AudioManager
            GameObject audioManager = GameObject.Find("AudioManager");
            if (audioManager == null)
            {
                audioManager = new GameObject("AudioManager");
            }

            // 4. Setup AudioSource
            AudioSource source = audioManager.GetComponent<AudioSource>();
            if (source == null)
            {
                source = audioManager.AddComponent<AudioSource>();
            }

            source.clip = musicClip;
            source.loop = true;
            source.playOnAwake = true;
            source.volume = 0.5f; // Set a reasonable default volume

            Debug.Log("[SetupBackgroundMusic] Successfully created AudioManager and set background music to loop!");
        }
    }
}
