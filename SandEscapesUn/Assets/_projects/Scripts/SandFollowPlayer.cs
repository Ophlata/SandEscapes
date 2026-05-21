using UnityEngine;

public class SandFollowPlayer : MonoBehaviour
{
    public Transform player;

    [Header("Смещение")]
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    [Header("Следовать только по XZ")]
    public bool followY = false;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = player.position + offset;

        if (!followY)
            targetPos.y = transform.position.y;

        transform.position = targetPos;
    }
}