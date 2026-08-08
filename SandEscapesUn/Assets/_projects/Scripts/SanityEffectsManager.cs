using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SandEscapes.Survival;

public class SanityEffectsManager : MonoBehaviour
{
    [Header("=== REFERENCES ===")]

    [Tooltip("Система характеристик игрока")]
    public PlayerStatsSystem playerStats;

    [Tooltip("Камера игрока")]
    public Camera playerCamera;

    // ============================================================
    // AUDIO
    // ============================================================

    [Header("=== AUDIO ===")]

    [Tooltip("AudioSource для шёпотов и галлюцинаций")]
    public AudioSource hallucinationAudio;

    [Tooltip("Шёпоты")]
    public AudioClip[] whisperSounds;

    [Tooltip("Странные звуки")]
    public AudioClip[] strangeSounds;

    [Tooltip("Звуки шагов/движения, которые игрок может слышать")]
    public AudioClip[] hallucinatedFootsteps;

    // ============================================================
    // SANITY THRESHOLDS
    // ============================================================

    [Header("=== SANITY THRESHOLDS ===")]

    [Tooltip("Выше этого значения никаких эффектов")]
    public float normalThreshold = 40f;

    [Tooltip("Начинаются странные звуки")]
    public float whisperThreshold = 40f;

    [Tooltip("Начинаются галлюцинации")]
    public float hallucinationThreshold = 25f;

    [Tooltip("Сильные эффекты")]
    public float dangerThreshold = 12f;

    // ============================================================
    // EFFECT FREQUENCY
    // ============================================================

    [Header("=== EFFECT FREQUENCY ===")]

    [Tooltip("Минимальная задержка между эффектами")]
    public float minimumEffectDelay = 3f;

    [Tooltip("Максимальная задержка между эффектами")]
    public float maximumEffectDelay = 8f;

    [Tooltip("Минимальная задержка при критическом рассудке")]
    public float criticalMinimumDelay = 1f;

    [Tooltip("Максимальная задержка при критическом рассудке")]
    public float criticalMaximumDelay = 4f;

    // ============================================================
    // SCREEN
    // ============================================================

    [Header("=== SCREEN EFFECT ===")]

    [Tooltip("UI Image на весь экран")]
    public Image screenFlash;

    [Range(0f, 1f)]
    public float normalFlashStrength = 0.08f;

    [Range(0f, 1f)]
    public float criticalFlashStrength = 0.18f;

    public float flashDuration = 0.08f;

    // ============================================================
    // CAMERA SHAKE
    // ============================================================

    [Header("=== CAMERA SHAKE ===")]

    public float normalShakeStrength = 0.01f;

    public float criticalShakeStrength = 0.035f;

    public float shakeDuration = 0.15f;

    // ============================================================
    // VISUAL HALLUCINATIONS
    // ============================================================

    [Header("=== VISUAL HALLUCINATIONS ===")]

    [Tooltip("Объекты с силуэтами, фигурами и т.п.")]
    public GameObject[] hallucinationObjects;

    [Tooltip("Сколько длится визуальная галлюцинация")]
    public float hallucinationDuration = 0.5f;

    [Tooltip("Минимальная дистанция появления")]
    public float hallucinationMinDistance = 5f;

    [Tooltip("Максимальная дистанция появления")]
    public float hallucinationMaxDistance = 20f;

    // ============================================================
    // OPTIONS
    // ============================================================

    [Header("=== OPTIONS ===")]

    public bool enableWhispers = true;
    public bool enableStrangeSounds = true;
    public bool enableScreenEffects = true;
    public bool enableCameraShake = true;
    public bool enableVisualHallucinations = true;

    // ============================================================
    // INTERNAL
    // ============================================================

    private float nextEffectTime;

    private Vector3 originalCameraPosition;

    private bool effectRunning;

    // ============================================================
    // START
    // ============================================================

    void Start()
    {
        if (playerStats == null)
        {
            playerStats =
                GetComponent<PlayerStatsSystem>();
        }

        if (playerCamera == null)
        {
            playerCamera =
                Camera.main;
        }

        if (playerCamera != null)
        {
            originalCameraPosition =
                playerCamera.transform.localPosition;
        }

        HideAllHallucinations();

        SetScreenAlpha(0f);

        ScheduleNextEffect();
    }

    // ============================================================
    // UPDATE
    // ============================================================

    void Update()
    {
        if (playerStats == null)
            return;

        if (playerStats.IsDead)
            return;

        float sanity =
            playerStats.Sanity;

        // Нормальный рассудок
        if (sanity > whisperThreshold)
            return;

        // Safe Zone полностью отключает эффекты
        if (playerStats.InSafeZone)
            return;

        if (Time.time >= nextEffectTime &&
            !effectRunning)
        {
            TriggerRandomEffect();

            ScheduleNextEffect();
        }
    }

    // ============================================================
    // SCHEDULE
    // ============================================================

    void ScheduleNextEffect()
    {
        float sanity =
            playerStats != null
                ? playerStats.Sanity
                : 100f;

        if (sanity <= dangerThreshold)
        {
            nextEffectTime =
                Time.time +
                Random.Range(
                    criticalMinimumDelay,
                    criticalMaximumDelay
                );
        }
        else
        {
            nextEffectTime =
                Time.time +
                Random.Range(
                    minimumEffectDelay,
                    maximumEffectDelay
                );
        }
    }

    // ============================================================
    // RANDOM EFFECT
    // ============================================================

    void TriggerRandomEffect()
    {
        float sanity =
            playerStats.Sanity;

        // ========================================================
        // 40-25
        // Только звуки
        // ========================================================

        if (sanity <= whisperThreshold &&
            sanity > hallucinationThreshold)
        {
            int effect =
                Random.Range(0, 2);

            if (effect == 0)
                PlayWhisper();
            else
                PlayStrangeSound();

            return;
        }

        // ========================================================
        // 25-12
        // Звуки + экран + галлюцинации
        // ========================================================

        if (sanity <= hallucinationThreshold &&
            sanity > dangerThreshold)
        {
            int effect =
                Random.Range(0, 5);

            switch (effect)
            {
                case 0:
                    PlayWhisper();
                    break;

                case 1:
                    PlayStrangeSound();
                    break;

                case 2:
                    StartCoroutine(ScreenFlicker());
                    break;

                case 3:
                    ShowRandomHallucination();
                    break;

                case 4:
                    StartCoroutine(CameraShake());
                    break;
            }

            return;
        }

        // ========================================================
        // <12
        // Всё сразу
        // ========================================================

        int dangerousEffect =
            Random.Range(0, 7);

        switch (dangerousEffect)
        {
            case 0:
                PlayWhisper();
                break;

            case 1:
                PlayStrangeSound();
                break;

            case 2:
                PlayHallucinatedFootsteps();
                break;

            case 3:
                StartCoroutine(ScreenFlicker());
                break;

            case 4:
                StartCoroutine(CameraShake());
                break;

            case 5:
                ShowRandomHallucination();
                break;

            case 6:
                StartCoroutine(
                    CombinedEffect()
                );
                break;
        }
    }

    // ============================================================
    // WHISPER
    // ============================================================

    void PlayWhisper()
    {
        if (!enableWhispers)
            return;

        if (hallucinationAudio == null)
            return;

        if (whisperSounds == null ||
            whisperSounds.Length == 0)
            return;

        AudioClip clip =
            whisperSounds[
                Random.Range(
                    0,
                    whisperSounds.Length
                )
            ];

        hallucinationAudio.pitch =
            Random.Range(
                0.85f,
                1.1f
            );

        hallucinationAudio.volume =
            Random.Range(
                0.25f,
                0.55f
            );

        hallucinationAudio.PlayOneShot(
            clip
        );
    }

    // ============================================================
    // STRANGE SOUND
    // ============================================================

    void PlayStrangeSound()
    {
        if (!enableStrangeSounds)
            return;

        if (hallucinationAudio == null)
            return;

        if (strangeSounds == null ||
            strangeSounds.Length == 0)
            return;

        AudioClip clip =
            strangeSounds[
                Random.Range(
                    0,
                    strangeSounds.Length
                )
            ];

        hallucinationAudio.pitch =
            Random.Range(
                0.75f,
                1.2f
            );

        hallucinationAudio.volume =
            Random.Range(
                0.2f,
                0.5f
            );

        hallucinationAudio.PlayOneShot(
            clip
        );
    }

    // ============================================================
    // HALLUCINATED FOOTSTEPS
    // ============================================================

    void PlayHallucinatedFootsteps()
    {
        if (hallucinationAudio == null)
            return;

        if (hallucinatedFootsteps == null ||
            hallucinatedFootsteps.Length == 0)
            return;

        AudioClip clip =
            hallucinatedFootsteps[
                Random.Range(
                    0,
                    hallucinatedFootsteps.Length
                )
            ];

        hallucinationAudio.pitch =
            Random.Range(
                0.9f,
                1.1f
            );

        hallucinationAudio.volume =
            Random.Range(
                0.2f,
                0.45f
            );

        hallucinationAudio.PlayOneShot(
            clip
        );
    }

    // ============================================================
    // SCREEN FLICKER
    // ============================================================

    IEnumerator ScreenFlicker()
    {
        if (!enableScreenEffects)
            yield break;

        if (screenFlash == null)
            yield break;

        effectRunning = true;

        float sanity =
            playerStats.Sanity;

        float strength =
            sanity <= dangerThreshold
                ? criticalFlashStrength
                : normalFlashStrength;

        SetScreenAlpha(
            strength
        );

        yield return new WaitForSeconds(
            flashDuration
        );

        SetScreenAlpha(0f);

        effectRunning = false;
    }

    // ============================================================
    // CAMERA SHAKE
    // ============================================================

    IEnumerator CameraShake()
    {
        if (!enableCameraShake)
            yield break;

        if (playerCamera == null)
            yield break;

        effectRunning = true;

        float sanity =
            playerStats.Sanity;

        float strength =
            sanity <= dangerThreshold
                ? criticalShakeStrength
                : normalShakeStrength;

        float timer = 0f;

        while (timer < shakeDuration)
        {
            Vector3 offset =
                Random.insideUnitSphere *
                strength;

            playerCamera.transform.localPosition =
                originalCameraPosition +
                offset;

            timer += Time.deltaTime;

            yield return null;
        }

        playerCamera.transform.localPosition =
            originalCameraPosition;

        effectRunning = false;
    }

    // ============================================================
    // VISUAL HALLUCINATION
    // ============================================================

    void ShowRandomHallucination()
    {
        if (!enableVisualHallucinations)
            return;

        if (hallucinationObjects == null ||
            hallucinationObjects.Length == 0)
            return;

        GameObject hallucination =
            hallucinationObjects[
                Random.Range(
                    0,
                    hallucinationObjects.Length
                )
            ];

        if (hallucination == null)
            return;

        StartCoroutine(
            HallucinationRoutine(
                hallucination
            )
        );
    }

    // ============================================================
    // HALLUCINATION ROUTINE
    // ============================================================

    IEnumerator HallucinationRoutine(
        GameObject hallucination)
    {
        effectRunning = true;

        if (hallucination == null)
        {
            effectRunning = false;
            yield break;
        }

        // --------------------------------------------------------
        // Если объект уже находится в сцене
        // --------------------------------------------------------

        hallucination.SetActive(true);

        float duration =
            hallucinationDuration;

        float sanity =
            playerStats.Sanity;

        if (sanity <= dangerThreshold)
        {
            duration *=
                Random.Range(
                    0.7f,
                    1.5f
                );
        }

        yield return new WaitForSeconds(
            duration
        );

        if (hallucination != null)
            hallucination.SetActive(false);

        effectRunning = false;
    }

    // ============================================================
    // COMBINED EFFECT
    // ============================================================

    IEnumerator CombinedEffect()
    {
        effectRunning = true;

        PlayWhisper();

        yield return new WaitForSeconds(
            Random.Range(
                0.1f,
                0.4f
            )
        );

        if (enableScreenEffects)
            StartCoroutine(
                ScreenFlicker()
            );

        yield return new WaitForSeconds(
            Random.Range(
                0.1f,
                0.3f
            )
        );

        if (enableVisualHallucinations)
            ShowRandomHallucination();

        effectRunning = false;
    }

    // ============================================================
    // SCREEN
    // ============================================================

    void SetScreenAlpha(float alpha)
    {
        if (screenFlash == null)
            return;

        Color color =
            screenFlash.color;

        color.a =
            Mathf.Clamp01(alpha);

        screenFlash.color =
            color;
    }

    // ============================================================
    // HIDE HALLUCINATIONS
    // ============================================================

    void HideAllHallucinations()
    {
        if (hallucinationObjects == null)
            return;

        foreach (
            GameObject obj
            in hallucinationObjects
        )
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    // ============================================================
    // RESET CAMERA
    // ============================================================

    void OnDisable()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition =
                originalCameraPosition;
        }

        SetScreenAlpha(0f);

        HideAllHallucinations();
    }
}