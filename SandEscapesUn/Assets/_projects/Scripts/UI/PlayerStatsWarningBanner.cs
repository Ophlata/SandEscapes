using System.Text;
using SandEscapes.Survival;
using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Shows combined warnings for low thirst/hunger/sanity and dangerous temperature.
    /// </summary>
    public class PlayerStatsWarningBanner : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] GameUiTheme theme;

        [SerializeField] PlayerStatsSystem stats;
        [SerializeField] Text bannerText;
        [Header("Overrides (optional)")]
        [SerializeField] Sprite warningBackgroundOverride;
        [SerializeField] Image.Type warningBackgroundImageType = Image.Type.Sliced;
        [SerializeField] Font fontOverride;
        [SerializeField] Color warningColor = new Color(1f, 0.45f, 0.35f, 1f);
        [SerializeField] Vector2 screenPadding = new Vector2(24f, 120f);
        [SerializeField] int sortOrder = 96;
        [SerializeField] int fontSize = 20;

        Image _background;

        void Awake()
        {
            if (stats == null)
                stats = GetComponent<PlayerStatsSystem>();
            EnsureText();
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

        void EnsureText()
        {
            if (bannerText != null)
            {
                ApplyFontToExisting();
                return;
            }

            if (stats == null)
                return;

            var canvasGo = new GameObject("SurvivalWarningsCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var rt = new GameObject("WarningBlock").AddComponent<RectTransform>();
            rt.SetParent(canvasGo.transform, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -screenPadding.y);
            rt.sizeDelta = new Vector2(-screenPadding.x * 2f, 80f);

            var bgSprite = UiThemeResolver.Sprite(warningBackgroundOverride, theme, theme != null ? theme.warningBannerBackgroundSprite : null);
            if (bgSprite != null && bgSprite != UiBuiltInSprites.White)
            {
                _background = rt.gameObject.AddComponent<Image>();
                _background.sprite = bgSprite;
                _background.type = theme != null ? theme.warningBannerImageType : warningBackgroundImageType;
                _background.color = new Color(1f, 1f, 1f, 0.92f);
            }

            var textGo = new GameObject("WarningText");
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 8f);
            textRt.offsetMax = new Vector2(-12f, -8f);

            bannerText = textGo.AddComponent<Text>();
            bannerText.font = UiThemeResolver.Font(fontOverride, theme);
            bannerText.fontSize = fontSize;
            bannerText.color = warningColor;
            bannerText.alignment = TextAnchor.UpperCenter;
            bannerText.supportRichText = true;
        }

        void ApplyFontToExisting()
        {
            if (bannerText != null)
                bannerText.font = UiThemeResolver.Font(fontOverride, theme);
        }

        void Refresh()
        {
            if (stats == null || bannerText == null)
                return;

            var sb = new StringBuilder();
            if (stats.IsThirstCritical)
                sb.Append("Low thirst! ");
            if (stats.IsHungerCritical)
                sb.Append("Low hunger! ");
            if (stats.IsSanityCritical)
                sb.Append("Low sanity! ");
            if (stats.IsTemperatureDanger)
                sb.Append("Dangerous temperature! ");

            var msg = sb.Length > 0 ? sb.ToString().TrimEnd() : string.Empty;
            bannerText.text = msg;
            bannerText.gameObject.SetActive(msg.Length > 0);
            if (_background != null)
                _background.enabled = msg.Length > 0;
        }
    }
}
