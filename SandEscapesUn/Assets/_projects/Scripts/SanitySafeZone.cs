using UnityEngine;
using SandEscapes.Survival;

[RequireComponent(typeof(BoxCollider))]
public class SanitySafeZone : MonoBehaviour
{
    [Header("=== SAFE ZONE ===")]

    [Tooltip("PlayerStatsSystem игрока")]
    public PlayerStatsSystem playerStats;

    [Tooltip("Показывать сообщение при входе/выходе")]
    public bool enableDebugLogs = false;

    private BoxCollider boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        // Safe Zone всегда должна быть Trigger
        boxCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (playerStats == null)
        {
            playerStats =
                other.GetComponentInParent<PlayerStatsSystem>();
        }

        if (playerStats == null)
        {
            Debug.LogWarning(
                "[SanitySafeZone] PlayerStatsSystem не найден на игроке.",
                this
            );

            return;
        }

        playerStats.InSafeZone = true;

        if (enableDebugLogs)
        {
            Debug.Log(
                "[SanitySafeZone] Игрок вошёл в безопасную зону.",
                this
            );
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (playerStats == null)
        {
            playerStats =
                other.GetComponentInParent<PlayerStatsSystem>();
        }

        if (playerStats == null)
            return;

        playerStats.InSafeZone = false;

        if (enableDebugLogs)
        {
            Debug.Log(
                "[SanitySafeZone] Игрок вышел из безопасной зоны.",
                this
            );
        }
    }

    bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;

        if (other.GetComponentInParent<PlayerStatsSystem>() != null)
            return true;

        return false;
    }

    void OnDisable()
    {
        // Чтобы игрок случайно не остался в Safe Zone,
        // если объект отключили во время игры.
        if (playerStats != null)
        {
            playerStats.InSafeZone = false;
        }
    }
}