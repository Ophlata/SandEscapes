using UnityEngine;

public class SanityNightSystem : MonoBehaviour
{
    [Header("=== РАССУДОК ===")]

    [Tooltip("Текущий рассудок")]
    public float sanity = 100f;

    [Tooltip("Максимальный рассудок")]
    public float maxSanity = 100f;

    [Header("=== НОЧЬ ===")]

    [Tooltip("Скорость падения рассудка ночью")]
    public float nightSanityLoss = 1.5f;

    [Tooltip("Минимальный рассудок")]
    public float minimumSanity = 0f;

    [Header("=== ДЕНЬ ===")]

    [Tooltip("Восстановление рассудка днём")]
    public float daySanityRecovery = 0.1f;

    [Header("=== ФАКЕЛ / СВЕТ ===")]

    [Tooltip("Находится ли игрок в безопасном свете")]
    public bool isInSafeLight = false;

    [Tooltip("Насколько свет уменьшает потерю рассудка")]
    [Range(0f, 1f)]
    public float lightProtection = 0.8f;

    void Update()
    {
        if (DayNightCycle.Instance == null)
            return;

        float nightIntensity =
            DayNightCycle.Instance.GetNightIntensity();

        if (nightIntensity > 0f)
        {
            float loss = nightSanityLoss * nightIntensity;

            // Свет защищает от потери рассудка
            if (isInSafeLight)
            {
                loss *= (1f - lightProtection);
            }

            sanity -= loss * Time.deltaTime;
        }
        else
        {
            // Днём медленно восстанавливаем рассудок
            sanity += daySanityRecovery * Time.deltaTime;
        }

        sanity = Mathf.Clamp(sanity, minimumSanity, maxSanity);
    }

    public float GetSanity01()
    {
        return sanity / maxSanity;
    }

    public bool IsDangerous()
    {
        return sanity <= 40f;
    }
}