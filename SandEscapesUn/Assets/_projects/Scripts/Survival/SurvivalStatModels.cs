using System;
using UnityEngine;

namespace SandEscapes.Survival
{
    /// <summary>Configurable vital (hunger, thirst, sanity, health).</summary>
    [Serializable]
    public class VitalStatChannel
    {
        [Min(0.01f)] public float maxValue = 100f;
        [Tooltip("Value when the scene starts and after respawn-style resets.")]
        public float currentValue = 100f;
        [Tooltip("Passive loss per second (0 for health if only indirect damage is used).")]
        public float decayPerSecond = 0.12f;
        [Tooltip("Multiplier for positive restores (items, passive regen).")]
        public float restoreRate = 1f;
        [Tooltip("At or below this absolute value the stat is treated as critically low.")]
        public float criticalThreshold = 22f;
        [Tooltip("At or below this value, stronger warnings / danger tier.")]
        public float dangerThreshold = 10f;
    }

    /// <summary>Temperature / exposure stress (0 = comfortable, max = extreme).</summary>
    [Serializable]
    public class TemperatureExposureChannel
    {
        [Min(0.01f)] public float maxStress = 100f;
        public float currentStress;
        [Tooltip("How fast current stress moves toward the ambient target.")]
        public float driftTowardsTargetPerSecond = 3f;
        [Tooltip("Target stress from the environment (inspector for tests, or set by zones later).")]
        public float ambientStressTarget;
        public float criticalThreshold = 65f;
        public float dangerThreshold = 82f;
    }

    /// <summary>Cross-links between vitals (rates are per second).</summary>
    [Serializable]
    public class SurvivalCrossSystemRules
    {
        [Header("Starvation")]
        [Tooltip("Health lost per second while thirst is at or below its critical threshold.")]
        public float healthDamagePerSecondFromCriticalThirst = 5f;
        [Tooltip("Health lost per second while hunger is at or below its critical threshold (slower than thirst).")]
        public float healthDamagePerSecondFromCriticalHunger = 1.75f;

        [Header("Exposure")]
        [Tooltip("Extra thirst decay multiplier at max stress (0 = none, 1 = doubles effective thirst loss at max).")]
        public float extraThirstDecayFromMaxStress = 0.85f;
        [Tooltip("Health lost per second when exposure is at or above its danger threshold.")]
        public float healthDamagePerSecondFromDangerTemperature = 2.5f;

        [Header("Sanity")]
        [Tooltip("Extra sanity decay per second per 'bad condition' (low hunger, low thirst, danger temperature).")]
        public float extraSanityDecayPerBadCondition = 0.35f;
        [Tooltip("Sanity restored per second while InSafeZone is true.")]
        public float passiveSanityRestorePerSecondInSafeZone = 1.25f;
        [Tooltip("When sanity is at or below its critical threshold, all passive decays are multiplied by this.")]
        public float globalDecayMultiplierWhenSanityCritical = 1.35f;
    }
}
