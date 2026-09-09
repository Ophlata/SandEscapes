using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Управление")]
    public float sensitivity = 2.0f;
    public float maxYAngle = 80.0f;

    [Header("Привязка к анимации")]
    public Transform playerBody;
    public Animator animator;
    public Vector3 eyeOffset;

    [Header("Look IK")]
    public float lookAtDistance = 5f;
    [Range(0, 1)] public float lookWeight = 1f;

    [Header("Динамика")]
    [Range(0, 1)] public float headInfluence = 0.5f; // сколько "живости" от кости головы подмешиваем

    private Transform headBone;
    private float rotationX = 0f;
    private Vector3 lookAtPoint;

    private void Start()
    {
        headBone = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        playerBody.Rotate(Vector3.up * mouseX);

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);

        Quaternion lookRotation = playerBody.rotation * Quaternion.Euler(rotationX, 0f, 0f);
        lookAtPoint = playerBody.position + Vector3.up * 1.6f + lookRotation * Vector3.forward * lookAtDistance;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetLookAtWeight(lookWeight, 0f, 1f, 1f, 0.5f);
        animator.SetLookAtPosition(lookAtPoint);
    }

    private void LateUpdate()
    {
        if (headBone == null) return;

        transform.position = headBone.position + headBone.TransformVector(eyeOffset);

        // Надёжный поворот от мыши — гарантирует полный диапазон
        Quaternion mouseRotation = playerBody.rotation * Quaternion.Euler(rotationX, 0f, 0f);

        // Подмешиваем реальный поворот кости — даёт "живость" анимации
        transform.rotation = Quaternion.Slerp(mouseRotation, headBone.rotation, headInfluence);
    }
}