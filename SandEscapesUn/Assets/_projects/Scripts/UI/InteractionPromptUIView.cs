using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Bottom-center hint text for interaction prompts.
    /// </summary>
    public class InteractionPromptUIView : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] GameUiTheme theme;

        [Header("Overrides (optional)")]
        [SerializeField] Font textFont;
        [SerializeField] Sprite promptBackgroundOverride;
        [SerializeField] Image.Type promptBackgroundImageType = Image.Type.Sliced;
        [SerializeField] float bottomPadding = 120f;
        [SerializeField] int fontSize = 20;
        [SerializeField] Color textColor = Color.white;

        Text label;
        GameObject root;
        Image background;

        void Awake()
        {
            textFont = UiThemeResolver.Font(textFont, theme);

            var canvasGo = new GameObject("PromptCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            root = new GameObject("PromptBlock");
            var rt = root.AddComponent<RectTransform>();
            rt.SetParent(canvasGo.transform, false);
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, bottomPadding);
            rt.sizeDelta = new Vector2(800f, 44f);

            var bgSprite = UiThemeResolver.Sprite(promptBackgroundOverride, theme, theme != null ? theme.interactionPromptBackgroundSprite : null);
            if (bgSprite != null && bgSprite != UiBuiltInSprites.White)
            {
                background = root.AddComponent<Image>();
                background.sprite = bgSprite;
                background.type = theme != null ? theme.interactionPromptImageType : promptBackgroundImageType;
                background.color = new Color(1f, 1f, 1f, 0.85f);
            }

            var textGo = new GameObject("PromptLabel");
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16f, 8f);
            textRt.offsetMax = new Vector2(-16f, -8f);

            label = textGo.AddComponent<Text>();
            label.font = textFont;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = textColor;
            Hide();
        }

        public void Show(string message)
        {
            if (label == null)
                return;
            label.text = message ?? string.Empty;
            root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
