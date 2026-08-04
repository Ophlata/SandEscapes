using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SteepSlopeController : MonoBehaviour
{
    [Header("Slope")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float slideSpeed = 8f;
    [SerializeField] private float rayLength = 1.5f;
    [SerializeField] private LayerMask groundMask = ~0;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!controller.isGrounded)
            return;

        if (!Physics.Raycast(transform.position + Vector3.up * 0.2f,
                Vector3.down,
                out RaycastHit hit,
                rayLength,
                groundMask))
            return;

        float angle = Vector3.Angle(hit.normal, Vector3.up);

        if (angle <= maxSlopeAngle)
            return;

        // Направление вниз по склону
        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

        controller.Move(slideDirection * slideSpeed * Time.deltaTime);
    }
}