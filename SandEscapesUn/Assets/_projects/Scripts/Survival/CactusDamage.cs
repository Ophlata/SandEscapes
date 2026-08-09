using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandEscapes.Survival;

namespace _projects.Scripts.Survival
{
    [RequireComponent(typeof(AudioSource))]
    public class CactusDamage : MonoBehaviour
    {
        [Header("Damage")] [SerializeField] private float damagePerTick = 5f;
        [SerializeField] private float damageInterval = 0.5f;

        [Header("Sound")] [SerializeField] private AudioClip hitSound;
        [SerializeField] [Range(0f, 1f)] private float hitVolume = 1f;

        [Header("Pushback")] [SerializeField] private float pushForce = 3f;
        [SerializeField] private float pushUpForce = 1f;

        [Header("Cactus Animation")] [SerializeField]
        private float shakeDistance = 0.05f;

        [SerializeField] private float shakeSpeed = 12f;

        [Header("Filter")] [SerializeField] private string playerTag = "Player";

        private AudioSource audioSource;
        private readonly Dictionary<PlayerStatsSystem, float> _nextDamageTime = new();

        private Vector3 startLocalPos;
        private Coroutine shakeRoutine;

        private Quaternion startRotation;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 1f;

            startRotation = transform.localRotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            PlayerStatsSystem stats = GetStats(other);

            if (stats == null || stats.IsDead)
                return;

            HitPlayer(other, stats);
            _nextDamageTime[stats] = Time.time + damageInterval;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            PlayerStatsSystem stats = GetStats(other);

            if (stats == null || stats.IsDead)
                return;

            if (!_nextDamageTime.ContainsKey(stats))
            {
                HitPlayer(other, stats);
                _nextDamageTime[stats] = Time.time + damageInterval;
                return;
            }

            if (Time.time >= _nextDamageTime[stats])
            {
                HitPlayer(other, stats);
                _nextDamageTime[stats] = Time.time + damageInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            PlayerStatsSystem stats = GetStats(other);

            if (stats != null)
                _nextDamageTime.Remove(stats);
        }

        private PlayerStatsSystem GetStats(Collider other)
        {
            PlayerStatsSystem stats = other.GetComponent<PlayerStatsSystem>();

            if (stats == null)
                stats = other.GetComponentInParent<PlayerStatsSystem>();

            return stats;
        }

        private void HitPlayer(Collider other, PlayerStatsSystem stats)
        {
            stats.TakeDamage(damagePerTick);

            if (hitSound != null)
                audioSource.PlayOneShot(hitSound, hitVolume);

            if (shakeRoutine != null)
                StopCoroutine(shakeRoutine);

            shakeRoutine = StartCoroutine(ShakeCactus());

            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller == null)
                controller = other.GetComponentInParent<CharacterController>();

            if (controller != null)
            {
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                pushDir.y = 0f;

                Vector3 push = pushDir * pushForce;
                push.y = pushUpForce;

                controller.Move(push * Time.deltaTime);
            }
        }

        private IEnumerator ShakeCactus()
        {
            Quaternion startRot = transform.localRotation;

            Vector3 axis = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)).normalized;

            float angle = Random.Range(4f, 7f);

            Quaternion hitRot = startRot * Quaternion.AngleAxis(angle, axis);

            // Наклон
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 18f;
                transform.localRotation = Quaternion.Slerp(startRot, hitRot, t);
                yield return null;
            }

            // Пружинящие колебания
            float duration = 0.35f;
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float wave = Mathf.Sin(timer * 35f);
                float fade = 1f - (timer / duration);

                transform.localRotation =
                    startRot *
                    Quaternion.AngleAxis(wave * angle * fade, axis);

                yield return null;
            }

            transform.localRotation = startRot;
        }
    }
}
        