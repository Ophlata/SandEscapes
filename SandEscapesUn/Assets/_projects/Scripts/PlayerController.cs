using UnityEngine;
using SandEscapes.Crafting;

public class PlayerController : MonoBehaviour
{
    [Header("Crafting")]
    [SerializeField] private CraftingUI craftingUI;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float gravity = -20f;
    public float jumpHeight = 2f;

    [Header("Water Movement")]
    public float waterWalkSpeed = 3f;
    public float waterRunSpeed = 5f;
    public float waterGravity = -5f;
    public float swimUpSpeed = 4f;
    public float swimDownSpeed = 3f;

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

    private bool isInWater;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
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
        if (Input.GetKeyDown(KeyCode.C))
            craftingUI?.Toggle();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? (isInWater ? waterRunSpeed : runSpeed)
            : (isInWater ? waterWalkSpeed : walkSpeed);

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);

        float inputMagnitude = move.magnitude;

        bool wasGrounded = controller.isGrounded;

        UpdateJumpTimers(wasGrounded);
        TryConsumeJump();

        float usedGravity = isInWater ? waterGravity : gravity;

        if (isInWater)
        {
            if (Input.GetKey(jumpKey))
                velocity.y = swimUpSpeed;
            else if (Input.GetKey(KeyCode.LeftControl))
                velocity.y = -swimDownSpeed;
            else
                velocity.y += usedGravity * Time.deltaTime;
        }
        else
        {
            velocity.y += usedGravity * Time.deltaTime;
        }

        Vector3 motion = move * speed;
        motion.y = velocity.y;

        CollisionFlags flags = controller.Move(motion * Time.deltaTime);

        bool isGrounded = (flags & CollisionFlags.Below) != 0;

        if (isGrounded && velocity.y < 0f && !isInWater)
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
        if (isInWater) return;

        if (jumpBufferTimer <= 0f) return;
        if (coyoteTimer <= 0f) return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        coyoteTimer = 0f;
        jumpBufferTimer = 0f;

        if (animator != null)
            animator.SetTrigger(jumpTriggerParamHash);
    }

    private void UpdateAnimation(float inputMagnitude, bool isGrounded)
    {
        if (animator == null) return;

        bool isMoving = inputMagnitude > moveInputThreshold;

        float targetSpeed = 0f;

        if (isMoving)
        {
            float maxSpeed = Mathf.Max(isInWater ? waterRunSpeed : runSpeed, 0.01f);
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ?
                (isInWater ? waterRunSpeed : runSpeed) :
                (isInWater ? waterWalkSpeed : walkSpeed);

            targetSpeed = Mathf.Clamp01((inputMagnitude * currentSpeed) / maxSpeed);
        }

        float normalizedSpeed = Mathf.SmoothDamp(
            animator.GetFloat(speedParamHash),
            targetSpeed,
            ref speedDampVelocity,
            speedDampTime
        );

        animator.SetFloat(speedParamHash, normalizedSpeed);
        animator.SetBool(movingParamHash, isMoving);
        animator.SetBool(groundedParamHash, isGrounded && !isInWater);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            velocity *= 0.5f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
        }
    }
}