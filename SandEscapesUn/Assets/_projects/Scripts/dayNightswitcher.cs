using UnityEngine;

public class AdvancedDayNightCycle : MonoBehaviour
{
    [Header("Time")]
    [Range(0f, 24f)] public float currentTime = 12f;
    public float fullDayDuration = 300f;

    [Header("Sun / Moon")]
    public Light sun;
    public Light moon;
    public float maxSunIntensity = 1.3f;
    public float maxMoonIntensity = 0.35f;

    [Header("Sky Colors")]
    public Gradient skyColor;
    public Gradient fogColor;
    public Gradient ambientColor;

    [Header("Fog")]
    public bool useFog = true;
    public float dayFogDensity = 0.005f;
    public float nightFogDensity = 0.025f;

    [Header("Stars")]
    public GameObject starsObject;

    [Header("Lamps")]
    public Light[] lamps;
    public float lampsOnTime = 19f;
    public float lampsOffTime = 6f;

    void Start()
    {
        RenderSettings.fog = useFog;
    }

    void Update()
    {
        UpdateTime();
        UpdateSunAndMoon();
        UpdateEnvironment();
        UpdateStars();
        UpdateLamps();
    }

    void UpdateTime()
    {
        currentTime += (24f / fullDayDuration) * Time.deltaTime;

        if (currentTime >= 24f)
            currentTime = 0f;
    }

    void UpdateSunAndMoon()
    {
        float sunAngle = (currentTime / 24f) * 360f - 90f;

        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

            float sunPower = Mathf.Clamp01(Mathf.Sin((currentTime - 6f) / 12f * Mathf.PI));
            sun.intensity = sunPower * maxSunIntensity;
            sun.enabled = sun.intensity > 0.01f;
        }

        if (moon != null)
        {
            moon.transform.rotation = Quaternion.Euler(sunAngle + 180f, 170f, 0f);

            float moonPower = 1f - Mathf.Clamp01(Mathf.Sin((currentTime - 6f) / 12f * Mathf.PI));
            moon.intensity = moonPower * maxMoonIntensity;
            moon.enabled = moon.intensity > 0.01f;
        }
    }

    void UpdateEnvironment()
    {
        float timePercent = currentTime / 24f;

        RenderSettings.ambientLight = ambientColor.Evaluate(timePercent);
        RenderSettings.fogColor = fogColor.Evaluate(timePercent);

        if (useFog)
        {
            bool isNight = currentTime >= 18.5f || currentTime <= 6f;
            RenderSettings.fogDensity = isNight ? nightFogDensity : dayFogDensity;
        }

        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = skyColor.Evaluate(timePercent);
    }

    void UpdateStars()
    {
        if (starsObject == null)
            return;

        bool night = currentTime >= 19f || currentTime <= 5f;
        starsObject.SetActive(night);
    }

    void UpdateLamps()
    {
        bool lampsShouldBeOn = currentTime >= lampsOnTime || currentTime <= lampsOffTime;

        foreach (Light lamp in lamps)
        {
            if (lamp != null)
                lamp.enabled = lampsShouldBeOn;
        }
    }
}
