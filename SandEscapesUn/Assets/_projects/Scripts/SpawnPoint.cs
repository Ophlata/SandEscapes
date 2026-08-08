using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("=== SPAWN POINT ===")]

    public string spawnID;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(
            transform.position,
            0.25f
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            transform.forward
        );
    }
}