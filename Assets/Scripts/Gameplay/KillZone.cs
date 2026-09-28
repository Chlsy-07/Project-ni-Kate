using UnityEngine;
using ProjectNiKate.Player;

namespace ProjectNiKate.Gameplay
{
    /// <summary>
    /// Placed on hazard GameObjects (spikes, death pits, etc.).
    /// Detects when the Player's HitBox (body/head) enters the trigger — 
    /// NOT the Player's physics body — so feet-first jumps near hazards
    /// are forgiven.
    ///
    /// Requires: a BoxCollider2D with Is Trigger = true on this GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only react if the PlayerHitBox child touched us —
            // not the root physics body (feet).
            if (collision.GetComponent<PlayerHitBox>() == null) return;

            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.Respawn();
            }
            else
            {
                Debug.LogWarning(
                    "[KillZone] Player HitBox entered kill zone but RespawnManager.Instance is null. " +
                    "Add a RespawnManager to the scene.");
            }
        }
    }
}
