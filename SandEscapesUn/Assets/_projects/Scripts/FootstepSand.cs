using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSand : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    public CharacterController controller;
    public AudioClip footstepLoop;

    [Header("=== MOVEMENT ===")]
    public float minInput = 0.1f;

    [Header("=== VOLUME ===")]
    [Tooltip("Максимальная громкость шагов")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.6f;

    [Tooltip("Скорость плавного появления звука")]
    public float fadeInSpeed = 4f;

    [Tooltip("Скорость плавного затухания звука")]
    public float fadeOutSpeed = 5f;

    [Header("=== STEP SPEED ===")]
    [Tooltip("Общий множитель скорости звука")]
    [Range(0.1f, 3f)]
    public float stepSoundSpeed = 1f;

    [Tooltip("Скорость звука при ходьбе")]
    [Range(0.1f, 3f)]
    public float walkPitch = 0.9f;

    [Tooltip("Скорость звука при беге")]
    [Range(0.1f, 3f)]
    public float runPitch = 1.3f;

    [Tooltip("Скорость игрока, при которой достигается максимальный pitch")]
    public float maxSpeed = 9f;

    [Header("=== AUDIO ===")]
    [Tooltip("Остановить AudioSource после полного затухания")]
    public bool stopAfterFadeOut = true;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.clip = footstepLoop;

        // Начинаем с нулевой громкости
        audioSource.volume = 0f;

        Debug.Log("Footstep initialized");
        Debug.Log("Clip = " + footstepLoop);
    }

    void Update()
    {
        if (controller == null)
            return;

        if (footstepLoop == null)
            return;

        // =====================================================
        // INPUT
        // =====================================================

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool hasInput =
            Mathf.Abs(h) > minInput ||
            Mathf.Abs(v) > minInput;

        bool shouldPlay =
            hasInput &&
            controller.isGrounded;

        // =====================================================
        // СКОРОСТЬ ИГРОКА
        // =====================================================

        float speed = controller.velocity.magnitude;

        float speedPercent =
            Mathf.Clamp01(speed / maxSpeed);

        // Ходьба -> бег
        float currentPitch = Mathf.Lerp(
            walkPitch,
            runPitch,
            speedPercent
        );

        // Общий множитель
        audioSource.pitch =
            currentPitch * stepSoundSpeed;

        // =====================================================
        // ПЛАВНЫЙ START
        // =====================================================

        if (shouldPlay)
        {
            // Если звук ещё не запущен
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Плавно увеличиваем громкость
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                footstepVolume,
                fadeInSpeed * Time.deltaTime
            );
        }

        // =====================================================
        // ПЛАВНЫЙ STOP
        // =====================================================

        else
        {
            // Плавно уменьшаем громкость
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                0f,
                fadeOutSpeed * Time.deltaTime
            );

            // Когда полностью затих
            if (stopAfterFadeOut &&
                audioSource.isPlaying &&
                audioSource.volume <= 0.001f)
            {
                audioSource.Stop();
                audioSource.volume = 0f;
            }
        }
    }

    // =========================================================
    // EDITOR / DISABLE
    // =========================================================

    void OnDisable()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }
    }
}