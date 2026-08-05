using System;
using UnityEngine;

namespace SandEscapes.Survival
{
    /// <summary>
    /// Central survival simulation: hunger, thirst, sanity, health, and temperature exposure with cross-system rules.
    /// </summary>
    public class PlayerStatsSystem : MonoBehaviour
    {
        [SerializeField] VitalStatChannel hunger = new VitalStatChannel
        {
            maxValue = 100f,
            currentValue = 85f,
            decayPerSecond = 0.08f,
            restoreRate = 1f,
            criticalThreshold = 22f,
            dangerThreshold = 10f
        };

        [SerializeField] VitalStatChannel thirst = new VitalStatChannel
        {
            maxValue = 100f,
            currentValue = 85f,
            decayPerSecond = 0.18f,
            restoreRate = 1f,
            criticalThreshold = 22f,
            dangerThreshold = 10f
        };

        [SerializeField] VitalStatChannel sanity = new VitalStatChannel
        {
            maxValue = 100f,
            currentValue = 100f,
            decayPerSecond = 0.04f,
            restoreRate = 1f,
            criticalThreshold = 25f,
            dangerThreshold = 12f
        };

        [SerializeField] VitalStatChannel health = new VitalStatChannel
        {
            maxValue = 100f,
            currentValue = 100f,
            decayPerSecond = 0f,
            restoreRate = 1f,
            criticalThreshold = 25f,
            dangerThreshold = 12f
        };

        [SerializeField] TemperatureExposureChannel exposure = new TemperatureExposureChannel();

        [SerializeField] SurvivalCrossSystemRules crossRules = new SurvivalCrossSystemRules();

        [Header("Environment")]
        [Tooltip("When true, sanity slowly recovers (safe camp / light / future triggers).")]
        [SerializeField] bool inSafeZone;

        [Header("Death")]
        [SerializeField] bool pauseSimulationWhenDead = true;

        [Header("Debug")]
        [SerializeField] bool enableDebugLogs;
        [SerializeField] bool freezeSimulation;
        [Tooltip("Multiplies simulation speed (decay, damage, drift) for faster testing.")]
        [SerializeField] [Min(0.01f)] float debugSimulationSpeed = 1f;

        float _hunger;
        float _thirst;
        float _sanity;
        float _health;
        bool _dead;

        public event Action OnStatsChanged;
        public event Action OnPlayerDied;

        public bool IsDead => _dead;
        public bool InSafeZone
        {
            get => inSafeZone;
            set => inSafeZone = value;
        }

        public float Hunger => _hunger;
        public float Thirst => _thirst;
        public float Sanity => _sanity;
        public float Health => _health;
        public float ExposureStress => exposure.currentStress;

        public float HungerNormalized => hunger.maxValue > 0.01f ? _hunger / hunger.maxValue : 0f;
        public float ThirstNormalized => thirst.maxValue > 0.01f ? _thirst / thirst.maxValue : 0f;
        public float SanityNormalized => sanity.maxValue > 0.01f ? _sanity / sanity.maxValue : 0f;
        public float HealthNormalized => health.maxValue > 0.01f ? _health / health.maxValue : 0f;
        public float ExposureNormalized => exposure.maxStress > 0.01f ? exposure.currentStress / exposure.maxStress : 0f;

        public VitalStatChannel HungerConfig => hunger;
        public VitalStatChannel ThirstConfig => thirst;
        public VitalStatChannel SanityConfig => sanity;
        public VitalStatChannel HealthConfig => health;
        public TemperatureExposureChannel ExposureConfig => exposure;

        public bool IsThirstCritical => _thirst <= thirst.criticalThreshold;
        public bool IsHungerCritical => _hunger <= hunger.criticalThreshold;
        public bool IsSanityCritical => _sanity <= sanity.criticalThreshold;
        public bool IsTemperatureDanger => exposure.currentStress >= exposure.dangerThreshold;
        public bool IsTemperatureCritical => exposure.currentStress >= exposure.criticalThreshold;

        void Awake()
        {
            ResetRuntimeFromInspectorDefaults();
        }

        void Update()
        {
            if (_dead && pauseSimulationWhenDead)
                return;
            if (freezeSimulation)
                return;

            Tick(Time.deltaTime * debugSimulationSpeed);
        }

        public void Tick(float dt)
        {
            if (dt <= 0f || _dead)
                return;

            DriftExposure(dt);

            var stressNorm = exposure.maxStress > 0.01f
                ? Mathf.Clamp01(exposure.currentStress / exposure.maxStress)
                : 0f;
            var thirstStressMult = 1f + stressNorm * crossRules.extraThirstDecayFromMaxStress;

            var badConditions = CountBadConditions();

            var sanityLowMult = _sanity <= sanity.criticalThreshold
                ? crossRules.globalDecayMultiplierWhenSanityCritical
                : 1f;

            _hunger -= hunger.decayPerSecond * sanityLowMult * dt;
            _thirst -= thirst.decayPerSecond * thirstStressMult * sanityLowMult * dt;

            var sanityDrain = sanity.decayPerSecond * sanityLowMult * dt;
            sanityDrain += badConditions * crossRules.extraSanityDecayPerBadCondition * dt;
            _sanity -= sanityDrain;

            if (inSafeZone)
                _sanity += crossRules.passiveSanityRestorePerSecondInSafeZone * sanity.restoreRate * dt;

            _health -= health.decayPerSecond * dt;

            if (_thirst <= thirst.criticalThreshold)
                _health -= crossRules.healthDamagePerSecondFromCriticalThirst * dt;
            if (_hunger <= hunger.criticalThreshold)
                _health -= crossRules.healthDamagePerSecondFromCriticalHunger * dt;

            if (exposure.currentStress >= exposure.dangerThreshold)
                _health -= crossRules.healthDamagePerSecondFromDangerTemperature * dt;

            ClampVitals();
            RaiseIfChanged();
            CheckDeath();
        }

        int CountBadConditions()
        {
            var n = 0;
            if (_hunger <= hunger.criticalThreshold)
                n++;
            if (_thirst <= thirst.criticalThreshold)
                n++;
            if (exposure.currentStress >= exposure.dangerThreshold)
                n++;
            return n;
        }

        void DriftExposure(float dt)
        {
            var target = Mathf.Clamp(exposure.ambientStressTarget, 0f, exposure.maxStress);
            var step = exposure.driftTowardsTargetPerSecond * dt;
            exposure.currentStress = Mathf.MoveTowards(exposure.currentStress, target, step);
        }

        void ClampVitals()
        {
            _hunger = Mathf.Clamp(_hunger, 0f, hunger.maxValue);
            _thirst = Mathf.Clamp(_thirst, 0f, thirst.maxValue);
            _sanity = Mathf.Clamp(_sanity, 0f, sanity.maxValue);
            _health = Mathf.Clamp(_health, 0f, health.maxValue);
        }

        void RaiseIfChanged()
        {
            OnStatsChanged?.Invoke();
        }

        void CheckDeath()
        {
            if (_health > 0.001f || _dead)
                return;

            _health = 0f;
            _dead = true;
            if (enableDebugLogs)
                Debug.Log("[PlayerStatsSystem] Player died (health reached 0).", this);
            OnPlayerDied?.Invoke();
            OnStatsChanged?.Invoke();
        }

        public void AddHunger(float amount)
        {
            if (amount <= 0f || _dead)
                return;
            _hunger = Mathf.Clamp(_hunger + amount * hunger.restoreRate, 0f, hunger.maxValue);
            OnStatsChanged?.Invoke();
        }

        public void AddThirst(float amount)
        {
            if (amount <= 0f || _dead)
                return;
            _thirst = Mathf.Clamp(_thirst + amount * thirst.restoreRate, 0f, thirst.maxValue);
            OnStatsChanged?.Invoke();
        }

        public void AddSanity(float amount)
        {
            if (amount <= 0f || _dead)
                return;
            _sanity = Mathf.Clamp(_sanity + amount * sanity.restoreRate, 0f, sanity.maxValue);
            OnStatsChanged?.Invoke();
        }

        public void AddHealth(float amount)
        {
            if (amount == 0f || _dead)
                return;
            _health = Mathf.Clamp(_health + amount * health.restoreRate, 0f, health.maxValue);
            OnStatsChanged?.Invoke();
        }
        public void TakeDamage(float damage)
        {
            if (_dead)
                return;

            _health -= damage;
            _health = Mathf.Clamp(_health, 0f, health.maxValue);

            OnStatsChanged?.Invoke();

            if (_health <= 0f)
                CheckDeath();
        }
        public void SetAmbientStressTarget(float target)
        {
            exposure.ambientStressTarget = Mathf.Clamp(target, 0f, exposure.maxStress);
        }

        public void SetExposureStress(float value)
        {
            exposure.currentStress = Mathf.Clamp(value, 0f, exposure.maxStress);
            OnStatsChanged?.Invoke();
        }

        /// <summary>Re-reads starting values from serialized channels (e.g. after scene reload).</summary>
        public void ResetRuntimeFromInspectorDefaults()
        {
            _dead = false;
            _hunger = Mathf.Clamp(hunger.currentValue, 0f, hunger.maxValue);
            _thirst = Mathf.Clamp(thirst.currentValue, 0f, thirst.maxValue);
            _sanity = Mathf.Clamp(sanity.currentValue, 0f, sanity.maxValue);
            _health = Mathf.Clamp(health.currentValue, 0f, health.maxValue);
            exposure.currentStress = Mathf.Clamp(exposure.currentStress, 0f, exposure.maxStress);
            OnStatsChanged?.Invoke();
        }

        public void ForceReviveAtFull()
        {
            _dead = false;
            _hunger = hunger.maxValue;
            _thirst = thirst.maxValue;
            _sanity = sanity.maxValue;
            _health = health.maxValue;
            exposure.currentStress = 0f;
            OnStatsChanged?.Invoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Print Stats")]
        void DebugPrintStats()
        {
            Debug.Log(
                $"Hunger {_hunger:F1}/{hunger.maxValue}  Thirst {_thirst:F1}/{thirst.maxValue}  " +
                $"Sanity {_sanity:F1}/{sanity.maxValue}  Health {_health:F1}/{health.maxValue}  " +
                $"Exposure {exposure.currentStress:F1}/{exposure.maxStress}  SafeZone={inSafeZone}",
                this);
        }

        [ContextMenu("Debug/Force Critical Thirst")]
        void DebugCriticalThirst()
        {
            _thirst = thirst.dangerThreshold;
            OnStatsChanged?.Invoke();
        }

        [ContextMenu("Debug/Force Critical Hunger")]
        void DebugCriticalHunger()
        {
            _hunger = hunger.dangerThreshold;
            OnStatsChanged?.Invoke();
        }

        [ContextMenu("Debug/Force Low Sanity")]
        void DebugLowSanity()
        {
            _sanity = sanity.dangerThreshold;
            OnStatsChanged?.Invoke();
        }

        [ContextMenu("Debug/Max Temperature Stress")]
        void DebugMaxTemperature()
        {
            exposure.currentStress = exposure.maxStress;
            exposure.ambientStressTarget = exposure.maxStress;
            OnStatsChanged?.Invoke();
        }

        [ContextMenu("Debug/Kill (Health to 0)")]
        void DebugKill()
        {
            _health = 0f;
            CheckDeath();
        }

        [ContextMenu("Debug/Revive Full")]
        void DebugRevive()
        {
            ForceReviveAtFull();
        }
#endif
    }
}
