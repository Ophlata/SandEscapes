using System.Collections.Generic;
using UnityEngine;
using SandEscapes.Survival;

namespace _projects.Scripts.Survival
{
    public class CactusDamage : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private float damagePerTick = 5f;
        [SerializeField] private float damageInterval = 0.5f;

        [Header("Pushback")]
        [SerializeField] private float pushForce = 3f;
        [SerializeField] private float pushUpForce = 1f;

        [Header("Filter")]
        [SerializeField] private string playerTag = "Player";

        private readonly Dictionary<PlayerStatsSystem, float> _nextDamageTime = new();

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerStatsSystem stats = GetStats(other);

            if (stats != null && _nextDamageTime.ContainsKey(stats))
                _nextDamageTime.Remove(stats);
        }

        private void TryDamage(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            PlayerStatsSystem stats = GetStats(other);

            if (stats == null || stats.IsDead)
                return;

            if (_nextDamageTime.ContainsKey(stats) && Time.time < _nextDamageTime[stats])
                return;

            HitPlayer(other, stats);
            _nextDamageTime[stats] = Time.time + damageInterval;
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
            stats.AddHealth(-damagePerTick);

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
    }
}