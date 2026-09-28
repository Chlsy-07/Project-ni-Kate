using UnityEngine;
using System.Collections;
using ProjectNiKate.Player;

namespace ProjectNiKate.Gameplay
{
    /// <summary>
    /// Singleton manager that handles player respawning.
    /// Place one instance in the scene. Assign a Spawn Point and
    /// optionally a Death Sound clip in the Inspector.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        public static RespawnManager Instance { get; private set; }

        [Tooltip("The world position the player returns to on death.")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("Optional audio clip played when the player dies.")]
        [SerializeField] private AudioClip _deathSound;

        private PlayerController _player;
        private AudioSource      _audioSource;
        private Vector3          _currentRespawnPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();

            if (_player == null)
                Debug.LogWarning("[RespawnManager] No PlayerController found in the scene.");

            if (_spawnPoint != null)
                _currentRespawnPosition = _spawnPoint.position;
        }

        /// <summary>
        /// Call this from a Checkpoint when the player reaches it.
        /// </summary>
        public void SetRespawnPoint(Vector3 newPosition)
        {
            _currentRespawnPosition = newPosition;
        }

        /// <summary>
        /// Teleports the player to the current respawn position and
        /// plays the death sound. Called by KillZone.
        /// </summary>
        public void Respawn()
        {
            if (_player == null)
            {
                Debug.LogWarning("[RespawnManager] Cannot respawn — PlayerController reference is null.");
                return;
            }

            // TeleportTo zeros velocity and moves the Rigidbody2D via _rb.position
            // so the physics engine respects the move on the same frame.
            _player.TeleportTo(_currentRespawnPosition);

            PlayDeathSound();

            Debug.Log($"[RespawnManager] Player respawned at {_currentRespawnPosition}");
        }

        private void PlayDeathSound()
        {
            if (_deathSound == null) return;

            _audioSource.clip = _deathSound;
            _audioSource.time = 0.30f; // Skip the first 0.30 s of the clip
            _audioSource.Play();

            StopAllCoroutines();
            StartCoroutine(StopSoundAfterDelay(2.7f));
        }

        private IEnumerator StopSoundAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_audioSource.isPlaying)
                _audioSource.Stop();
        }
    }
}
