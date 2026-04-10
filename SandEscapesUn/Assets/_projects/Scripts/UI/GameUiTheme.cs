using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Central place for HUD sprites and font. Assign one asset on UI roots, then override per-component if needed.
    /// </summary>
    [CreateAssetMenu(fileName = "GameUiTheme", menuName = "Sand Escapes/UI/Game UI Theme", order = 0)]
    public class GameUiTheme : ScriptableObject
    {
        [Header("Core")]
        [Tooltip("Replaces the default 1×1 quad everywhere the theme does not specify a sprite.")]
        public Sprite neutralQuadSprite;
        public Font uiFont;

        [Header("Hotbar")]
        public Sprite hotbarSlotFrameSprite;
        [Tooltip("Shown behind item icons when empty.")]
        public Sprite hotbarIconPlaceholderSprite;

        [Header("Survival stat bars")]
        public Sprite statBarTrackSprite;
        public Sprite statBarFillSprite;
        public Sprite statIconHunger;
        public Sprite statIconThirst;
        public Sprite statIconSanity;
        public Sprite statIconHealth;

        [Header("Stat bar labels")]
        [Tooltip("If false, generated HUD hides text labels (icons only if icons are set).")]
        public bool statBarsShowTextLabels = true;

        [Header("Warnings banner")]
        public Sprite warningBannerBackgroundSprite;
        public Image.Type warningBannerImageType = Image.Type.Sliced;

        [Header("Interaction prompt")]
        [Tooltip("Optional panel behind the prompt text.")]
        public Sprite interactionPromptBackgroundSprite;
        public Image.Type interactionPromptImageType = Image.Type.Sliced;

        [Header("Inventory panel")]
        public Sprite inventoryPanelBackgroundSprite;
        public Sprite inventoryCellFrameSprite;

        [Header("Loot panel")]
        public Sprite lootPanelBackgroundSprite;
        public Sprite lootButtonSprite;
    }
}
