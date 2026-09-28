using UnityEngine;
using ProjectNiKate.Gameplay;
using ProjectNiKate.Player;

namespace ProjectNiKate.Gameplay.Environment
{
    /// <summary>
    /// Attach to a flag/checkpoint GameObject (e.g. tile-sheet_51).
    /// When the player's root collider overlaps the trigger, the global
    /// respawn position is updated and the "Activate" animator trigger fires.
    ///
    /// Requires: BoxCollider2D with Is Trigger = true on this GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        private bool     _isActivated;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Fire on the ROOT Player collider — not the HitBox child.
            // PlayerController lives on the root, so checking for it here
            // means the flag activates when the player's main body passes through.
            if (_isActivated) return;
            if (collision.GetComponent<PlayerController>() == null) return;

            _isActivated = true;

            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.SetRespawnPoint(transform.position);
                Debug.Log($"[Checkpoint] Activated at {transform.position}");
            }
            else
            {
                Debug.LogWarning("[Checkpoint] RespawnManager not found in scene.");
            }

            if (_animator != null)
                _animator.SetTrigger("Activate");
        }
    }
}
