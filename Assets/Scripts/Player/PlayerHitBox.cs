using UnityEngine;

namespace ProjectNiKate.Player
{
    /// <summary>
    /// Marker component on the Player's HitBox child object.
    /// KillZone checks for this component to confirm the player's
    /// vulnerable area (body/head) touched a hazard — not just their feet.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerHitBox : MonoBehaviour
    {
    }
}
