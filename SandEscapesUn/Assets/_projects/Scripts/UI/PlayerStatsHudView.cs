using SandEscapes.Survival;
using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Four vital bars (hunger, thirst, sanity, health). Generates a minimal HUD when bar references are not assigned.
    /// </summary>
    public class PlayerStatsHudView : MonoBehaviour
    {
        [Header("Theme")]
        [Tooltip("Assign a Game UI Theme asset for default sprites/font, or leave empty and use overrides / placeholders.")]
        [SerializeField] GameUiTheme theme;

        [SerializeField] PlayerStatsSystem stats;

        [Header("Manual bars (optional)")]
        [SerializeField] Image hungerFill;
        [SerializeField] Image thirstFill;
        [SerializeField] Image sanityFill;
        [SerializeField] Image healthFill;

        [Header("Sprite overrides (optional, beat theme)")]
        [SerializeField] Sprite statBarTrackOverride;
        [SerializeField] Sprite statBarFillOverride;
        [SerializeField] Sprite statIconHungerOverride;
        [SerializeField] Sprite statIconThirstOverride;
        [SerializeField] Sprite statIconSanityOverride;
        [SerializeField] Sprite statIconHealthOverride;
        [SerializeField] Font statLabelFontOverride;

        [Header("Labels")]
        [Tooltip("If off, text names are hidden (icons only when icons are set). Theme can also hide labels.")]
        [SerializeField] bool showStatTextLabels = true;

        [Header("Generated layout")]
        [SerializeField] Vector2 barSize = new Vector2(220f, 18f);
        [SerializeField] float iconColumnWidth = 32f;
        [SerializeField] float rowSpacing = 6f;
        [SerializeField] Vector2 screenPadding = new Vector2(24f, 24f);
        [SerializeField] int sortOrder = 95;
        [SerializeField] Color hungerColor = new Color(0.85f, 0.55f, 0.2f, 0.95f);
        [SerializeField] Color thirstColor = new Color(0.25f, 0.55f, 0.95f, 0.95f);
        [SerializeField] Color sanityColor = new Color(0.65f, 0.35f, 0.85f, 0.95f);
        [SerializeField] Color healthColor = new Color(0.35f, 0.85f, 0.35f, 0.95f);
        [SerializeField] Color statLabelColor = new Color(1f, 1f, 1f, 0.92f);
        [SerializeField] Color statBarBackgroundTint = new Color(0.08f, 0.08f, 0.08f, 0.75f);
        [SerializeField] Image.Type statBarTrackImageType = Image.Type.Simple;

        BarRefs _runtime;

        void Awake()
        {
            if (stats == null)
                stats = GetComponent<PlayerStatsSystem>();
            EnsureBars();
        }

        void OnEnable()
        {
            if (stats != null)
                stats.OnStatsChanged += Refresh;
        }

        void OnDisable()
        {
            if (stats != null)
                stats.OnStatsChanged -= Refresh;
        }

        void Start()
        {
            Refresh();
        }

        void EnsureBars()
        {
            if (hungerFill != null && thirstFill != null && sanityFill != null && healthFill != null)
                return;
            if (hungerFill != null || thirstFill != null || sanityFill != null || healthFill != null)
            {
                Debug.LogWarning($"{nameof(PlayerStatsHudView)}: assign all four bar Images or leave all empty for generated HUD.", this);
                return;
            }

            if (stats == null)
            {
                Debug.LogError($"{nameof(PlayerStatsHudView)} needs {nameof(PlayerStatsSystem)}.", this);
                return;
            }

            _runtime = BuildRuntimeHud();
            hungerFill = _runtime.Hunger;
            thirstFill = _runtime.Thirst;
            sanityFill = _runtime.Sanity;
            healthFill = _runtime.Health;
        }

        void Refresh()
        {
            if (stats == null)
                return;
            if (hungerFill != null)
                hungerFill.fillAmount = stats.HungerNormalized;
            if (thirstFill != null)
                thirstFill.fillAmount = stats.ThirstNormalized;
            if (sanityFill != null)
                sanityFill.fillAmount = stats.SanityNormalized;
            if (healthFill != null)
                healthFill.fillAmount = stats.HealthNormalized;
        }

        bool EffectiveShowLabels()
        {
            if (!showStatTextLabels)
                return false;
            if (theme != null && !theme.statBarsShowTextLabels)
                return false;
            return true;
        }

        Sprite TrackSprite() => UiThemeResolver.Sprite(statBarTrackOverride, theme, theme != null ? theme.statBarTrackSprite : null);
        Sprite FillSprite() => UiThemeResolver.Sprite(statBarFillOverride, theme, theme != null ? theme.statBarFillSprite : null);
        Font LabelFont() => UiThemeResolver.Font(statLabelFontOverride, theme);

        BarRefs BuildRuntimeHud()
        {
            var canvasGo = new GameObject("SurvivalStatsCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = new GameObject("StatsColumn").AddComponent<RectTransform>();
            root.SetParent(canvasGo.transform, false);
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = new Vector2(screenPadding.x, -screenPadding.y);

            var col = root.gameObject.AddComponent<VerticalLayoutGroup>();
            col.spacing = rowSpacing;
            col.childAlignment = TextAnchor.UpperLeft;
            col.childControlWidth = true;
            col.childControlHeight = false;
            col.childForceExpandWidth = true;
            col.childForceExpandHeight = false;

            var track = TrackSprite();
            var fill = FillSprite();

            return new BarRefs
            {
                Hunger = CreateBarRow(root, "Hunger", hungerColor, UiThemeResolver.Sprite(statIconHungerOverride, theme, theme != null ? theme.statIconHunger : null), track, fill),
                Thirst = CreateBarRow(root, "Thirst", thirstColor, UiThemeResolver.Sprite(statIconThirstOverride, theme, theme != null ? theme.statIconThirst : null), track, fill),
                Sanity = CreateBarRow(root, "Sanity", sanityColor, UiThemeResolver.Sprite(statIconSanityOverride, theme, theme != null ? theme.statIconSanity : null), track, fill),
                Health = CreateBarRow(root, "Health", healthColor, UiThemeResolver.Sprite(statIconHealthOverride, theme, theme != null ? theme.statIconHealth : null), track, fill)
            };
        }

        Image CreateBarRow(RectTransform parent, string label, Color fillColor, Sprite iconSprite, Sprite trackSprite, Sprite fillSpriteResolved)
        {
            var row = new GameObject(label + "_Row");
            var rowRt = row.AddComponent<RectTransform>();
            rowRt.SetParent(parent, false);
            rowRt.sizeDelta = new Vector2(barSize.x + iconColumnWidth + 8f, barSize.y + 4f);

            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandHeight = true;

            var hasIcon = iconSprite != null;
            if (hasIcon)
            {
                var iconGo = new GameObject("Icon");
                var iconRt = iconGo.AddComponent<RectTransform>();
                iconRt.SetParent(rowRt, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.preferredWidth = iconColumnWidth;
                iconLe.preferredHeight = iconColumnWidth;
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = iconSprite;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
            }

            if (EffectiveShowLabels())
            {
                var labelGo = new GameObject("Label");
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.SetParent(rowRt, false);
                labelRt.sizeDelta = new Vector2(72f, barSize.y);
                var le = labelGo.AddComponent<LayoutElement>();
                le.preferredWidth = 72f;
                var txt = labelGo.AddComponent<Text>();
                txt.font = LabelFont();
                txt.fontSize = 15;
                txt.color = statLabelColor;
                txt.text = label;
                txt.alignment = TextAnchor.MiddleLeft;
            }

            var bgGo = new GameObject("Background");
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.SetParent(rowRt, false);
            var bgLe = bgGo.AddComponent<LayoutElement>();
            bgLe.preferredWidth = barSize.x;
            bgLe.preferredHeight = barSize.y;
            var bg = bgGo.AddComponent<Image>();
            bg.sprite = trackSprite;
            bg.type = statBarTrackImageType;
            bg.color = statBarBackgroundTint;

            var fillGo = new GameObject("Fill");
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.SetParent(bgRt, false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fill = fillGo.AddComponent<Image>();
            fill.sprite = fillSpriteResolved;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.color = fillColor;
            fill.fillAmount = 1f;

            return fill;
        }

        class BarRefs
        {
            public Image Hunger;
            public Image Thirst;
            public Image Sanity;
            public Image Health;
        }
    }
}
