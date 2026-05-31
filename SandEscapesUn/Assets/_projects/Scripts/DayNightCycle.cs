using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    [Header("=== СВЕТ ===")]
    public Light sun;
    public Light moon;
    public ReflectionProbe reflectionProbe;

    [Header("=== ВИЗУАЛЬНЫЕ ОБЪЕКТЫ ===")]
    public MeshRenderer sunDisc;
    public MeshRenderer moonDisc;
    public MeshRenderer starSphere;

    [Header("=== ТЕКСТУРЫ ===")]
    public Texture2D sunTexture;
    public Texture2D moonTexture;
    public Texture2D starTexture;

    [Header("=== НАСТРОЙКИ СОЛНЦА ===")]
    public Color sunDiscDayColor    = new Color(1.00f, 0.95f, 0.70f);
    public Color sunDiscGoldenColor = new Color(1.00f, 0.45f, 0.10f);
    public float sunDiscSize     = 15f;
    public float sunDiscDistance = 200f;

    [Header("=== НАСТРОЙКИ ЛУНЫ ===")]
    public Color moonDiscColor   = new Color(0.85f, 0.90f, 1.00f);
    public float moonDiscSize    = 12f;
    public float moonDiscDistance= 200f;

    [Header("=== НАСТРОЙКИ ЗВЁЗД ===")]
    public Color starColor          = new Color(0.80f, 0.85f, 1.00f);
    public float starRotationSpeed  = 1.5f;

    [Header("=== ВРЕМЯ ===")]
    [Range(0f, 24f)] public float currentHour = 8f;
    public float dayDuration = 600f;
    public bool  runCycle    = true;

    [Header("=== ЦВЕТА СВЕТА ===")]
    public Color sunriseColor    = new Color(1.00f, 0.55f, 0.20f);
    public Color noonColor       = new Color(1.00f, 0.97f, 0.85f);
    public Color sunsetColor     = new Color(1.00f, 0.38f, 0.10f);
    public float noonIntensity   = 2.8f;
    public float goldenIntensity = 1.2f;
    [Range(-90f, 90f)] public float sunTiltAngle  =  25f;
    public Color moonLightColor  = new Color(0.60f, 0.70f, 1.00f);
    [Range(0f, 1f)]  public float moonMaxIntensity = 0.12f;
    [Range(-90f, 90f)] public float moonTiltAngle = -20f;

    [Header("=== AMBIENT ===")]
    public Color dayAmbientColor    = new Color(0.95f, 0.82f, 0.55f);
    public Color sunsetAmbientColor = new Color(0.60f, 0.25f, 0.10f);
    public Color moonAmbientColor   = new Color(0.03f, 0.04f, 0.10f);
    public Color nightAmbientColor  = new Color(0.01f, 0.01f, 0.04f);
    [Range(0f, 2f)] public float ambientIntensity = 1.0f;

    [Header("=== ТУМАН ===")]
    public bool  controlFog      = true;
    public Color dayFogColor     = new Color(0.98f, 0.90f, 0.70f);
    public Color sunsetFogColor  = new Color(0.80f, 0.35f, 0.15f);
    public Color nightFogColor   = new Color(0.01f, 0.01f, 0.04f);
    public float dayFogDensity   = 0.002f;
    public float nightFogDensity = 0.015f;

    [Header("=== СКАЙБОКС ===")]
    public bool useProceedSkybox = true;

    // ══════════════════════════════════════════════════════
    //  СВЕТЛЯЧКИ И НОЧНОЙ ТУМАН
    //  Активны с вечера (~18:00) до утра (~7:00)
    // ══════════════════════════════════════════════════════
    [Header("=== СВЕТЛЯЧКИ И НОЧНОЙ ТУМАН ===")]

    [Tooltip("ParticleSystem светлячков — назначь в Inspector")]
    public ParticleSystem fireflies;

    [Tooltip("Час начала fade-in светлячков (напр. 18 = 18:00)")]
    [Range(0f, 24f)] public float firefliesFadeInStart  = 18f;

    [Tooltip("Час полной яркости светлячков (напр. 20 = 20:00)")]
    [Range(0f, 24f)] public float firefliesFadeInEnd    = 20f;

    [Tooltip("Час начала fade-out светлячков утром (напр. 5 = 05:00)")]
    [Range(0f, 24f)] public float firefliesFadeOutStart = 5f;

    [Tooltip("Час полного исчезновения светлячков (напр. 7 = 07:00)")]
    [Range(0f, 24f)] public float firefliesFadeOutEnd   = 7f;

    [Tooltip("Максимальное кол-во частиц светлячков в ночное время")]
    [Min(0)] public int firefliesMaxParticles = 200;

    [Tooltip("Включить отдельный ночной туман поверх обычного")]
    public bool useNightFog = true;

    [Tooltip("Цвет ночного тумана (синеватый, мистический)")]
    public Color nightMistColor   = new Color(0.04f, 0.06f, 0.15f);

    [Tooltip("Плотность ночного тумана")]
    public float nightMistDensity = 0.025f;

    // ══════════════════════════════════════════════════════
    //  СОБЫТИЯ
    // ══════════════════════════════════════════════════════
    public System.Action OnSunrise;
    public System.Action OnNoon;
    public System.Action OnSunset;
    public System.Action OnMidnight;

    // ── приватные ──
    private float    _normalizedTime;
    private Material _skyboxMat;
    private int      _lastHourInt = -1;
    private Material _sunDiscMat;
    private Material _moonDiscMat;
    private Material _starMat;

    // ══════════════════════════════════════════════════════
    void Start()
    {
        _skyboxMat = RenderSettings.skybox;
        if (controlFog) RenderSettings.fog = true;
        InitDiscMaterials();
        ApplyAll();
    }

    void Update()
    {
        if (runCycle && Application.isPlaying)
        {
            currentHour += (24f / dayDuration) * Time.deltaTime;
            if (currentHour >= 24f) currentHour -= 24f;
        }
        ApplyAll();
        FireHourEvents();
    }

    // ══════════════════════════════════════════════════════
    void ApplyAll()
    {
        _normalizedTime = currentHour / 24f;

        RotateSun();
        RotateMoon();
        RotateStars();

        ApplySunLight();
        ApplyMoonLight();
        ApplyAmbient();

        UpdateSunDisc();
        UpdateMoonDisc();
        UpdateStars();

        if (controlFog) ApplyFog();
        if (useProceedSkybox && _skyboxMat != null) ApplySkybox();

        // Светлячки и ночной туман
        ApplyFireflies();
        if (useNightFog) ApplyNightAtmosphere();

        if (reflectionProbe != null && Application.isPlaying)
            reflectionProbe.RenderProbe();
    }

    // ══════════════════════════════════════════════════════
    //  СВЕТЛЯЧКИ
    //  Считаем alpha [0..1] по текущему часу:
    //  18→20 fade in | 20→05 полная яркость | 05→07 fade out
    // ══════════════════════════════════════════════════════
    void ApplyFireflies()
    {
        if (fireflies == null) return;

        float alpha = GetNightAlpha(
            firefliesFadeInStart, firefliesFadeInEnd,
            firefliesFadeOutStart, firefliesFadeOutEnd);

        if (!Application.isPlaying) return; // В Editor не трогаем частицы

        var emission = fireflies.emission;
        var main     = fireflies.main;

        if (alpha > 0.01f)
        {
            if (!fireflies.isPlaying) fireflies.Play();
            emission.enabled = true;
            // Плавно меняем кол-во частиц через rateOverTime
            emission.rateOverTime = Mathf.RoundToInt(firefliesMaxParticles * alpha / 10f);

            // Прозрачность через startColor
            Color c = main.startColor.color;
            c.a = alpha;
            main.startColor = c;
        }
        else
        {
            emission.enabled = false;
            if (fireflies.isPlaying) fireflies.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    // ══════════════════════════════════════════════════════
    //  НОЧНОЙ ТУМАН (поверх ApplyFog)
    //  Смешивает ночной цвет/плотность с тем, что уже выставил ApplyFog
    // ══════════════════════════════════════════════════════
    void ApplyNightAtmosphere()
    {
        if (!controlFog) return;

        float alpha = GetNightAlpha(
            firefliesFadeInStart, firefliesFadeInEnd,
            firefliesFadeOutStart, firefliesFadeOutEnd);

        if (alpha <= 0f) return;

        // Принудительно перезаписываем, не lerp поверх
        RenderSettings.fogColor   = Color.Lerp(RenderSettings.fogColor, nightMistColor, alpha);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, nightMistDensity, alpha);
        RenderSettings.fogMode    = FogMode.ExponentialSquared;
    }

    // ══════════════════════════════════════════════════════
    //  УТИЛИТА: считает alpha [0..1] для вечерне-ночного окна
    //  с учётом перехода через полночь (0:00)
    // ══════════════════════════════════════════════════════
    float GetNightAlpha(float fadeInStart, float fadeInEnd,
                        float fadeOutStart, float fadeOutEnd)
    {
        float h = currentHour;

        // Вечерний fade-in (18 → 20)
        if (h >= fadeInStart && h < fadeInEnd)
            return Mathf.InverseLerp(fadeInStart, fadeInEnd, h);

        // Полная ночь (20 → 24 и 0 → 5)
        if (h >= fadeInEnd || h < fadeOutStart)
            return 1f;

        // Утренний fade-out (5 → 7)
        if (h >= fadeOutStart && h < fadeOutEnd)
            return 1f - Mathf.InverseLerp(fadeOutStart, fadeOutEnd, h);

        return 0f; // День
    }

    // ══════════════════════════════════════════════════════
    //  ИНИЦИАЛИЗАЦИЯ МАТЕРИАЛОВ
    // ══════════════════════════════════════════════════════
    void InitDiscMaterials()
    {
        if (sunDisc != null)
        {
            _sunDiscMat = new Material(Shader.Find("Unlit/Transparent"));
            if (sunTexture != null) _sunDiscMat.mainTexture = sunTexture;
            sunDisc.sharedMaterial = _sunDiscMat;
            sunDisc.transform.localScale = Vector3.one * sunDiscSize;
        }
        if (moonDisc != null)
        {
            _moonDiscMat = new Material(Shader.Find("Unlit/Transparent"));
            if (moonTexture != null) _moonDiscMat.mainTexture = moonTexture;
            moonDisc.sharedMaterial = _moonDiscMat;
            moonDisc.transform.localScale = Vector3.one * moonDiscSize;
        }
        if (starSphere != null)
        {
            _starMat = new Material(Shader.Find("Unlit/Transparent"));
            if (starTexture != null) _starMat.mainTexture = starTexture;
            starSphere.sharedMaterial = _starMat;
        }
    }

    // ══════════════════════════════════════════════════════
    void RotateSun()
    {
        if (sun == null) return;
        sun.transform.rotation = Quaternion.Euler(
            _normalizedTime * 360f - 90f, sunTiltAngle, 0f);
    }

    void RotateMoon()
    {
        if (moon == null) return;
        moon.transform.rotation = Quaternion.Euler(
            (_normalizedTime + 0.5f) * 360f - 90f, moonTiltAngle, 0f);
    }

    void RotateStars()
    {
        if (starSphere == null) return;
        float angle = currentHour * starRotationSpeed;
        starSphere.transform.rotation = Quaternion.Euler(0f, angle, 23.5f);
    }

    void ApplySunLight()
    {
        if (sun == null) return;
        float h = SunHeight();
        Color col; float intensity;

        if (h > 0.3f)
        {
            float t = Mathf.InverseLerp(0.3f, 1.0f, h);
            col = Color.Lerp(sunriseColor, noonColor, t);
            intensity = Mathf.Lerp(goldenIntensity, noonIntensity, t);
        }
        else if (h > -0.1f)
        {
            float t = Mathf.InverseLerp(-0.1f, 0.3f, h);
            col = Color.Lerp(Color.black, currentHour < 12f ? sunriseColor : sunsetColor, t);
            intensity = Mathf.Lerp(0f, goldenIntensity, t);
        }
        else { col = Color.black; intensity = 0f; }

        sun.color     = col;
        sun.intensity = intensity;
        sun.enabled   = intensity > 0.01f;
        sun.shadows   = intensity > 0.3f ? LightShadows.Soft : LightShadows.None;
    }

    void ApplyMoonLight()
    {
        if (moon == null) return;
        float moonH    = MoonHeight();
        float sunBlend = Mathf.Clamp01((-SunHeight() - 0.05f) / 0.15f);
        float intensity = moonH > 0f
            ? moonMaxIntensity * Mathf.Clamp01(moonH / 0.2f) * sunBlend
            : 0f;

        moon.color     = moonLightColor;
        moon.intensity = intensity;
        moon.enabled   = intensity > 0.001f;
        moon.shadows   = LightShadows.Soft;
    }

    void UpdateSunDisc()
    {
        if (sunDisc == null || _sunDiscMat == null) return;
        float h = SunHeight();
        Color discColor; float alpha;

        if (h > 0.1f)
        {
            float t = Mathf.InverseLerp(0.1f, 1.0f, h);
            discColor = Color.Lerp(sunDiscGoldenColor, sunDiscDayColor, t);
            alpha = 1f;
        }
        else if (h > -0.05f)
        {
            discColor = sunDiscGoldenColor;
            alpha = Mathf.InverseLerp(-0.05f, 0.1f, h);
        }
        else { discColor = sunDiscGoldenColor; alpha = 0f; }

        discColor.a = alpha;
        _sunDiscMat.color = discColor;
        sunDisc.enabled = alpha > 0.01f;
        PositionDiscAlongLight(sunDisc.transform, sun.transform, sunDiscDistance);
    }

    void UpdateMoonDisc()
    {
        if (moonDisc == null || _moonDiscMat == null) return;
        float moonH    = MoonHeight();
        float sunBlend = Mathf.Clamp01((-SunHeight() - 0.05f) / 0.15f);
        float alpha;

        if (moonH > 0.05f)
            alpha = Mathf.Clamp01(moonH / 0.15f) * sunBlend;
        else if (moonH > -0.05f)
            alpha = Mathf.InverseLerp(-0.05f, 0.05f, moonH) * sunBlend;
        else
            alpha = 0f;

        Color col = moonDiscColor; col.a = alpha;
        _moonDiscMat.color = col;
        moonDisc.enabled = alpha > 0.01f;
        PositionDiscAlongLight(moonDisc.transform, moon.transform, moonDiscDistance);
    }

    void UpdateStars()
    {
        if (starSphere == null || _starMat == null) return;
        float h = SunHeight();
        float alpha = h < -0.1f ? 1f : h < 0.1f ? Mathf.InverseLerp(0.1f, -0.1f, h) : 0f;
        Color col = starColor; col.a = alpha;
        _starMat.color = col;
        starSphere.enabled = alpha > 0.01f;
    }

    void PositionDiscAlongLight(Transform disc, Transform lightTransform, float distance)
    {
        if (Camera.main == null) return;
        Vector3 dir = -lightTransform.forward;
        disc.position = Camera.main.transform.position + dir * distance;
        disc.LookAt(Camera.main.transform.position);
    }

    void ApplyAmbient()
    {
        float sunH = SunHeight(); float moonH = MoonHeight();
        Color ambient;

        if (sunH > 0.3f)
            ambient = Color.Lerp(sunsetAmbientColor, dayAmbientColor, Mathf.InverseLerp(0.3f, 1.0f, sunH));
        else if (sunH > -0.1f)
            ambient = Color.Lerp(moonAmbientColor, sunsetAmbientColor, Mathf.InverseLerp(-0.1f, 0.3f, sunH));
        else if (moonH > 0.1f)
            ambient = Color.Lerp(nightAmbientColor, moonAmbientColor, Mathf.InverseLerp(0.1f, 0.8f, moonH));
        else
            ambient = nightAmbientColor;

        RenderSettings.ambientLight = ambient * ambientIntensity;
        RenderSettings.ambientMode  = AmbientMode.Flat;
    }

    void ApplyFog()
    {
        float t01 = Mathf.Clamp01((SunHeight() + 1f) * 0.5f);
        Color fogColor; float fogDensity;

        if (t01 > 0.6f)
        {
            float t = Mathf.InverseLerp(0.6f, 1f, t01);
            fogColor   = Color.Lerp(sunsetFogColor, dayFogColor, t);
            fogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, t);
        }
        else if (t01 > 0.2f)
        {
            float t = Mathf.InverseLerp(0.2f, 0.6f, t01);
            fogColor   = Color.Lerp(nightFogColor, sunsetFogColor, t);
            fogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity * 2f, t);
        }
        else { fogColor = nightFogColor; fogDensity = nightFogDensity; }

        RenderSettings.fogColor   = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogMode    = FogMode.ExponentialSquared;
    }

    void ApplySkybox()
    {
        if (_skyboxMat == null) return;
        float t01 = Mathf.Clamp01((SunHeight() + 1f) * 0.5f);

        if (_skyboxMat.HasProperty("_AtmosphereThickness"))
            _skyboxMat.SetFloat("_AtmosphereThickness", Mathf.Lerp(0f, 1.0f, t01));
        if (_skyboxMat.HasProperty("_Exposure"))
            _skyboxMat.SetFloat("_Exposure", Mathf.Lerp(0.05f, 1.8f, t01));
        if (_skyboxMat.HasProperty("_GroundColor"))
            _skyboxMat.SetColor("_GroundColor", Color.Lerp(
                new Color(0.01f, 0.01f, 0.03f), new Color(0.72f, 0.57f, 0.32f), t01));
        if (_skyboxMat.HasProperty("_SunSize"))
            _skyboxMat.SetFloat("_SunSize", 0.04f);

        DynamicGI.UpdateEnvironment();
    }

    float SunHeight()  => Mathf.Sin(_normalizedTime * Mathf.PI * 2f - Mathf.PI * 0.5f);
    float MoonHeight() => Mathf.Sin((_normalizedTime + 0.5f) * Mathf.PI * 2f - Mathf.PI * 0.5f);

    void FireHourEvents()
    {
        int h = Mathf.FloorToInt(currentHour);
        if (h == _lastHourInt) return;
        _lastHourInt = h;
        switch (h)
        {
            case 6:  OnSunrise?.Invoke();  break;
            case 12: OnNoon?.Invoke();     break;
            case 20: OnSunset?.Invoke();   break;
            case 0:  OnMidnight?.Invoke(); break;
        }
    }

    public float GetNormalizedTime() => _normalizedTime;
    public bool  IsDay()    => SunHeight() > 0f;
    public bool  IsNight()  => !IsDay();
    public bool  IsMoonUp() => MoonHeight() > 0f;
    public void  SetTime(float hour) => currentHour = Mathf.Repeat(hour, 24f);

    public void SetSunTexture(Texture2D tex)
    {
        sunTexture = tex;
        if (_sunDiscMat != null) _sunDiscMat.mainTexture = tex;
    }

    public void SetMoonTexture(Texture2D tex)
    {
        moonTexture = tex;
        if (_moonDiscMat != null) _moonDiscMat.mainTexture = tex;
    }

    public void SetStarTexture(Texture2D tex)
    {
        starTexture = tex;
        if (_starMat != null) _starMat.mainTexture = tex;
    }
}