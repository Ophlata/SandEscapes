using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = -20f;
    public float jumpHeight = 2f;

    [Header("Jump")]
    public float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    private float coyoteTimer;
    private float jumpBufferTimer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string movingParam = "IsMoving";
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string jumpTriggerParam = "Jump";
    [SerializeField] [Range(0.01f, 1f)] private float moveInputThreshold = 0.1f;
    [SerializeField] [Range(0f, 0.5f)] private float speedDampTime = 0.08f;

    private int speedParamHash;
    private int movingParamHash;
    private int groundedParamHash;
    private int jumpTriggerParamHash;
    private float speedDampVelocity;

    private CharacterController controller;
    private Vector3 velocity;

    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        speedParamHash = Animator.StringToHash(speedParam);
        movingParamHash = Animator.StringToHash(movingParam);
        groundedParamHash = Animator.StringToHash(groundedParam);
        jumpTriggerParamHash = Animator.StringToHash(jumpTriggerParam);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        move = Vector3.ClampMagnitude(move, 1f);
        var inputMagnitude = move.magnitude;
        var wasGrounded = controller.isGrounded;

        UpdateJumpTimers(wasGrounded);
        TryConsumeJump();

        velocity.y += gravity * Time.deltaTime;
        var motion = move * speed;
        motion.y = velocity.y;
        var flags = controller.Move(motion * Time.deltaTime);
        var isGrounded = (flags & CollisionFlags.Below) != 0;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        UpdateAnimation(inputMagnitude, isGrounded);
    }

    private void UpdateJumpTimers(bool isGrounded)
    {
        if (Input.GetKeyDown(jumpKey))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
    }

    private void TryConsumeJump()
    {
        if (jumpBufferTimer <= 0f)
            return;
        if (coyoteTimer <= 0f)
            return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        if (animator != null)
            animator.SetTrigger(jumpTriggerParamHash);
    }

    private void UpdateAnimation(float inputMagnitude, bool isGrounded)
    {
        if (animator == null)
            return;

        bool isMoving = inputMagnitude > moveInputThreshold;
        float targetSpeed = 0f;
        if (isMoving)
        {
            var maxSpeed = Mathf.Max(runSpeed, 0.01f);
            targetSpeed = Mathf.Clamp01((inputMagnitude * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed)) / maxSpeed);
        }
        float normalizedSpeed = Mathf.SmoothDamp(animator.GetFloat(speedParamHash), targetSpeed, ref speedDampVelocity, speedDampTime);

        animator.SetFloat(speedParamHash, normalizedSpeed);
        animator.SetBool(movingParamHash, isMoving);
        animator.SetBool(groundedParamHash, isGrounded);
    }
}