using UnityEngine;

public class FireLightFlicker : MonoBehaviour
{
    [SerializeField] private Light fireLight;

    [Header("Intensity")]
    [SerializeField] private float minIntensity = 2f;
    [SerializeField] private float maxIntensity = 4f;

    [Header("Flicker")]
    [SerializeField] private float flickerSpeed = 5f;

    private float randomOffset;

    private void Start()
    {
        randomOffset = Random.Range(0f, 999f);

        if (fireLight == null)
            fireLight = GetComponent<Light>();
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);

        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}