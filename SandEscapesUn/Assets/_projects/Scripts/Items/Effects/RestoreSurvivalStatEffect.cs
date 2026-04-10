using UnityEngine;
using SandEscapes.Survival;

namespace SandEscapes.Items.Effects
{
    [CreateAssetMenu(fileName = "RestoreStat", menuName = "Sand Escapes/Items/Effects/Restore Survival Stat", order = 1)]
    public class RestoreSurvivalStatEffect : ItemUseEffect
    {
        public enum SurvivalStatKind
        {
            Thirst = 0,
            Hunger = 1,
            Sanity = 2,
            Health = 3
        }

        [SerializeField] SurvivalStatKind stat;
        [SerializeField] float amount = 25f;

        public override bool TryApply(GameObject user, ItemData itemUsed)
        {
            if (user == null)
                return false;

            var stats = user.GetComponentInParent<PlayerStatsSystem>();
            if (stats == null)
                stats = user.GetComponent<PlayerStatsSystem>();
            if (stats == null)
                return false;

            switch (stat)
            {
                case SurvivalStatKind.Thirst:
                    stats.AddThirst(amount);
                    return true;
                case SurvivalStatKind.Hunger:
                    stats.AddHunger(amount);
                    return true;
                case SurvivalStatKind.Sanity:
                    stats.AddSanity(amount);
                    return true;
                case SurvivalStatKind.Health:
                    stats.AddHealth(amount);
                    return true;
                default:
                    return false;
            }
        }
    }
}
