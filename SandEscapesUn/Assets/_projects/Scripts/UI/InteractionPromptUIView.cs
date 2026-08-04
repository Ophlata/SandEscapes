using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    [ExecuteAlways]
    public class InteractionPromptUIView : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] GameUiTheme theme;

        [Header("Позиция (от центра экрана)")]
        [Tooltip("Смещение относительно центра экрана. Y > 0 — выше центра, Y < 0 — ниже центра.")]
        [SerializeField] Vector2 offsetFromCenter = new Vector2(0f, -80f);

        [Header("Размер блока")]
        [SerializeField] Vector2 boxSize = new Vector2(600f, 56f);
        [SerializeField] Vector2 textPadding = new Vector2(16f, 8f);

        [Header("Текст")]
        [SerializeField] Font textFont;
        [SerializeField] int fontSize = 24;
        [SerializeField] FontStyle fontStyle = FontStyle.Bold;
        [SerializeField] Color textColor = Color.white;

        [Header("Фон (опционально)")]
        [SerializeField] bool showBackground = true;
        [SerializeField] Sprite promptBackgroundOverride;
        [SerializeField] Image.Type promptBackgroundImageType = Image.Type.Sliced;
        [SerializeField] Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);

        [Header("Сортировка")]
        [SerializeField] int sortingOrder = 150;

        [Header("Предпросмотр в редакторе")]
        [Tooltip("Показывать блок в редакторе (вне Play Mode), чтобы удобно настраивать позицию")]
        [SerializeField] bool previewInEditor = true;

        Text label;
        GameObject root;
        Image background;
        RectTransform rt;

        void Awake()
        {
            BuildIfNeeded();
            ApplyLayout();

            if (Application.isPlaying)
                Hide();
            else if (previewInEditor)
                Show("Пример подсказки");
        }

        void OnValidate()
        {
            // OnValidate может вызваться раньше Awake в редакторе — подстрахуемся
            if (root == null)
                return;

            ApplyLayout();
        }

        void BuildIfNeeded()
        {
            if (root != null)
                return;

            textFont = UiThemeResolver.Font(textFont, theme);

            var canvasGo = new GameObject("PromptCanvas");
            canvasGo.hideFlags = HideFlags.DontSave;
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            root = new GameObject("PromptBlock");
            rt = root.AddComponent<RectTransform>();
            rt.SetParent(canvasGo.transform, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            if (showBackground)
            {
                var bgSprite = UiThemeResolver.Sprite(promptBackgroundOverride, theme, theme != null ? theme.interactionPromptBackgroundSprite : null);
                background = root.AddComponent<Image>();
                background.sprite = bgSprite != UiBuiltInSprites.White ? bgSprite : null;
                background.type = theme != null ? theme.interactionPromptImageType : promptBackgroundImageType;
                background.color = backgroundColor;
            }

            var textGo = new GameObject("PromptLabel");
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;

            label = textGo.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
        }

        void ApplyLayout()
        {
            if (rt == null)
                rt = root.GetComponent<RectTransform>();

            rt.anchoredPosition = offsetFromCenter;
            rt.sizeDelta = boxSize;

            var textRt = label.GetComponent<RectTransform>();
            textRt.offsetMin = textPadding;
            textRt.offsetMax = -textPadding;

            label.font = textFont;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.color = textColor;

            if (background != null)
                background.color = backgroundColor;
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