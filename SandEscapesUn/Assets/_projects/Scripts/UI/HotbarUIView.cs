using System;
using SandEscapes.Inventory;
using SandEscapes.Items;
using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Hotbar presentation for the first N inventory slots. Generates a minimal overlay UI when manual bindings are not provided.
    /// </summary>
    public class HotbarUIView : MonoBehaviour
    {
        [Header("Theme")]
        [Tooltip("Shared sprites/font. Per-field overrides below win over the theme.")]
        [SerializeField] GameUiTheme theme;

        [Header("Sources")]
        [SerializeField] InventorySystem inventory;
        [SerializeField] HotbarSystem hotbar;

        [Header("Manual slots (optional)")]
        [SerializeField] HotbarSlotViewBinding[] manualSlotBindings;

        [Header("Sprite overrides (optional)")]
        [SerializeField] Sprite hotbarSlotFrameOverride;
        [SerializeField] Sprite hotbarIconPlaceholderOverride;

        [Header("Generated layout (when manual count mismatches hotbar size)")]
        [SerializeField] Vector2 slotSize = new Vector2(72f, 72f);
        [SerializeField] float slotSpacing = 8f;
        [SerializeField] float bottomScreenPadding = 24f;
        [SerializeField] Color frameNormalColor = new Color(0.15f, 0.15f, 0.15f, 0.92f);
        [SerializeField] Color frameSelectedColor = new Color(0.95f, 0.75f, 0.15f, 1f);
        [SerializeField] Color hotkeyNormalColor = Color.white;
        [SerializeField] int hotkeyFontSize = 16;
        [SerializeField] int stackFontSize = 18;
        [SerializeField] Font textFont;

        SlotVisuals[] visuals;

        void OnEnable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged += Refresh;
            if (hotbar != null)
                hotbar.OnSelectionChanged += Refresh;

            if (visuals != null)
                Refresh();
        }

        void OnDisable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= Refresh;
            if (hotbar != null)
                hotbar.OnSelectionChanged -= Refresh;
        }

        void Start()
        {
            EnsureVisuals();
            Refresh();
        }

        void EnsureVisuals()
        {
            if (inventory == null || hotbar == null)
            {
                Debug.LogError($"{nameof(HotbarUIView)} requires {nameof(InventorySystem)} and {nameof(HotbarSystem)}.", this);
                return;
            }

            textFont = UiThemeResolver.Font(textFont, theme);

            var hotbarSlots = inventory.HotbarSlotCount;
            if (manualSlotBindings != null && manualSlotBindings.Length == hotbarSlots)
                visuals = BuildFromBindings(manualSlotBindings);
            else
            {
                if (manualSlotBindings != null && manualSlotBindings.Length > 0)
                    Debug.LogWarning($"{nameof(HotbarUIView)}: manual slot count ({manualSlotBindings.Length}) != hotbar ({hotbarSlots}). Using generated UI.", this);

                visuals = BuildRuntimeSlots(hotbarSlots);
            }
        }

        void Refresh()
        {
            if (inventory == null || hotbar == null || visuals == null)
                return;

            var selected = hotbar.SelectedHotbarIndex;
            for (var i = 0; i < visuals.Length; i++)
                ApplySlotVisual(visuals[i], inventory.GetSlot(i), i + 1, i == selected);
        }

        void ApplySlotVisual(SlotVisuals v, InventorySlot slot, int displayNumber, bool selected)
        {
            if (v.Frame != null)
                v.Frame.color = selected ? frameSelectedColor : frameNormalColor;

            if (v.Hotkey != null)
            {
                v.Hotkey.text = displayNumber.ToString();
                v.Hotkey.color = hotkeyNormalColor;
                v.Hotkey.font = textFont;
                v.Hotkey.fontSize = hotkeyFontSize;
            }

            if (v.Icon != null)
            {
                var hasItem = !slot.IsEmpty && slot.Item != null;
                v.Icon.enabled = hasItem;
                v.Icon.sprite = hasItem ? slot.Item.Icon : null;
                v.Icon.color = Color.white;
            }

            if (v.Count != null)
            {
                var showStack = !slot.IsEmpty && slot.Quantity > 1;
                v.Count.gameObject.SetActive(showStack);
                if (showStack)
                {
                    v.Count.text = slot.Quantity.ToString();
                    v.Count.font = textFont;
                    v.Count.fontSize = stackFontSize;
                }
            }
        }

        SlotVisuals[] BuildFromBindings(HotbarSlotViewBinding[] bindings)
        {
            var result = new SlotVisuals[bindings.Length];
            for (var i = 0; i < bindings.Length; i++)
            {
                var b = bindings[i];
                result[i] = new SlotVisuals
                {
                    Frame = b.frameImage,
                    Icon = b.iconImage,
                    Count = b.quantityText,
                    Hotkey = b.hotkeyText
                };
            }

            return result;
        }

        SlotVisuals[] BuildRuntimeSlots(int count)
        {
            var canvasGo = new GameObject("HotbarCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var barRt = new GameObject("HotbarRow").AddComponent<RectTransform>();
            barRt.SetParent(canvasGo.transform, false);
            barRt.anchorMin = new Vector2(0.5f, 0f);
            barRt.anchorMax = new Vector2(0.5f, 0f);
            barRt.pivot = new Vector2(0.5f, 0f);
            barRt.anchoredPosition = new Vector2(0f, bottomScreenPadding);

            var row = barRt.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = slotSpacing;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var slots = new SlotVisuals[count];
            for (var i = 0; i < count; i++)
                slots[i] = CreateRuntimeSlot(barRt, i + 1);

            return slots;
        }

        SlotVisuals CreateRuntimeSlot(RectTransform parent, int number)
        {
            var root = new GameObject($"HotbarSlot_{number}");
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.SetParent(parent, false);
            rootRt.sizeDelta = slotSize;

            var frame = root.AddComponent<Image>();
            frame.sprite = UiThemeResolver.Sprite(hotbarSlotFrameOverride, theme, theme != null ? theme.hotbarSlotFrameSprite : null);
            frame.type = Image.Type.Simple;
            frame.color = frameNormalColor;

            var iconGo = new GameObject("Icon");
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.SetParent(rootRt, false);
            iconRt.anchorMin = new Vector2(0.1f, 0.15f);
            iconRt.anchorMax = new Vector2(0.9f, 0.85f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            var icon = iconGo.AddComponent<Image>();
            icon.sprite = UiThemeResolver.Sprite(hotbarIconPlaceholderOverride, theme, theme != null ? theme.hotbarIconPlaceholderSprite : null);
            icon.preserveAspect = true;
            icon.enabled = false;

            var keyGo = new GameObject("Hotkey");
            var keyRt = keyGo.AddComponent<RectTransform>();
            keyRt.SetParent(rootRt, false);
            keyRt.anchorMin = new Vector2(0f, 1f);
            keyRt.anchorMax = new Vector2(0f, 1f);
            keyRt.pivot = new Vector2(0f, 1f);
            keyRt.anchoredPosition = new Vector2(6f, -4f);
            keyRt.sizeDelta = new Vector2(28f, 28f);
            var keyText = keyGo.AddComponent<Text>();
            keyText.font = textFont;
            keyText.fontSize = hotkeyFontSize;
            keyText.alignment = TextAnchor.UpperLeft;
            keyText.color = hotkeyNormalColor;
            keyText.text = number.ToString();

            var countGo = new GameObject("StackCount");
            var countRt = countGo.AddComponent<RectTransform>();
            countRt.SetParent(rootRt, false);
            countRt.anchorMin = new Vector2(1f, 0f);
            countRt.anchorMax = new Vector2(1f, 0f);
            countRt.pivot = new Vector2(1f, 0f);
            countRt.anchoredPosition = new Vector2(-6f, 6f);
            countRt.sizeDelta = new Vector2(40f, 28f);
            var countText = countGo.AddComponent<Text>();
            countText.font = textFont;
            countText.fontSize = stackFontSize;
            countText.alignment = TextAnchor.LowerRight;
            countText.color = Color.white;
            countText.gameObject.SetActive(false);

            return new SlotVisuals
            {
                Frame = frame,
                Icon = icon,
                Count = countText,
                Hotkey = keyText
            };
        }

        /// <summary>1×1 white quad; prefer assigning sprites on <see cref="GameUiTheme"/> or overrides.</summary>
        public static Sprite GetSharedUiSprite() => UiBuiltInSprites.White;

        [Serializable]
        class SlotVisuals
        {
            public Image Frame;
            public Image Icon;
            public Text Count;
            public Text Hotkey;
        }
    }
}
