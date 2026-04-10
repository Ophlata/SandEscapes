using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Optional wired references for one hotbar slot (used instead of runtime-generated UI).
    /// </summary>
    [System.Serializable]
    public class HotbarSlotViewBinding
    {
        public Image frameImage;
        public Image iconImage;
        public Text quantityText;
        public Text hotkeyText;
    }
}
