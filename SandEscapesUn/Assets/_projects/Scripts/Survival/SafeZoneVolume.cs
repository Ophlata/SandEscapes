using UnityEngine;

namespace SandEscapes.Survival
{
    /// <summary>
    /// Marks a trigger volume as a "safe" area where <see cref="PlayerStatsSystem.InSafeZone"/> is true (sanity regen).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SafeZoneVolume : MonoBehaviour
    {
        void Reset()
        {
            var c = GetComponent<Collider>();
            c.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            var stats = other.GetComponentInParent<PlayerStatsSystem>();
            if (stats != null)
                stats.InSafeZone = true;
        }

        void OnTriggerExit(Collider other)
        {
            var stats = other.GetComponentInParent<PlayerStatsSystem>();
            if (stats != null)
                stats.InSafeZone = false;
        }
    }
}
