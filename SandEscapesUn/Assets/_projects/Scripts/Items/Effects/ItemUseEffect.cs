using UnityEngine;

namespace SandEscapes.Items.Effects
{
    /// <summary>
    /// Pluggable effect when a consumable is used. Create derived assets and assign them on <see cref="ItemData"/>.
    /// </summary>
    public abstract class ItemUseEffect : ScriptableObject
    {
        public abstract bool TryApply(GameObject user, ItemData itemUsed);
    }
}
