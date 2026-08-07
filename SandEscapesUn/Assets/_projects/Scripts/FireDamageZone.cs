using System.Collections.Generic;
using UnityEngine;
using SandEscapes.Survival;

[RequireComponent(typeof(AudioSource))]
public class FireDamageZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePerTick = 10f;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Sound")]
    [SerializeField] private AudioClip burnSound;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private AudioSource audioSource;

    private readonly Dictionary<PlayerStatsSystem, float> nextDamageTime = new();
    private readonly HashSet<PlayerStatsSystem> soundPlayed = new();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        PlayerStatsSystem stats = GetStats(other);

        if (stats == null || stats.IsDead)
            return;

        if (!soundPlayed.Contains(stats))
        {
            soundPlayed.Add(stats);

            if (burnSound != null)
                audioSource.PlayOneShot(burnSound, volume);
        }

        DamagePlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        DamagePlayer(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        PlayerStatsSystem stats = GetStats(other);

        if (stats != null)
        {
            nextDamageTime.Remove(stats);
            soundPlayed.Remove(stats);
        }
    }

    private void DamagePlayer(Collider other)
    {
        PlayerStatsSystem stats = GetStats(other);

        if (stats == null || stats.IsDead)
            return;

        if (!nextDamageTime.TryGetValue(stats, out float nextTime))
        {
            stats.TakeDamage(damagePerTick);
            nextDamageTime[stats] = Time.time + damageInterval;
            return;
        }

        if (Time.time >= nextTime)
        {
            stats.TakeDamage(damagePerTick);
            nextDamageTime[stats] = Time.time + damageInterval;
        }
    }

    private PlayerStatsSystem GetStats(Collider other)
    {
        PlayerStatsSystem stats = other.GetComponent<PlayerStatsSystem>();

        if (stats == null)
            stats = other.GetComponentInParent<PlayerStatsSystem>();

        return stats;
    }
}