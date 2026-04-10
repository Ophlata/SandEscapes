using System.Text;
using SandEscapes.Items;
using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Minimal loot window: list text + Take all + Close.
    /// Layout values below apply when the UI is built at runtime (edit in Inspector, then enter Play).
    /// </summary>
    public class LootPanelUIView : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] GameUiTheme theme;

        [Header("Fonts")]
        [SerializeField] Font textFont;

        [Header("Input")]
        [SerializeField] KeyCode closeKey = KeyCode.Escape;
        [Tooltip("Extra key to close the panel (e.g. Q).")]
        [SerializeField] KeyCode alternateCloseKey = KeyCode.Q;

        [Header("Layout (runtime-built panel)")]
        [SerializeField] Vector2 panelSize = new Vector2(420f, 320f);
        [SerializeField] Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] int canvasSortOrder = 200;
        [SerializeField] Color panelBackgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        [SerializeField] Color buttonColor = new Color(0.25f, 0.25f, 0.28f, 1f);

        [Header("Custom graphics (optional, override theme)")]
        [Tooltip("Фон панели. Пусто = белый квадрат (тинтится цветом выше). Для 9-slice включите Panel Image Type = Sliced.")]
        [SerializeField] Sprite panelBackgroundSprite;
        [SerializeField] Image.Type panelImageType = Image.Type.Simple;
        [Tooltip("Кнопки Take all / Close. Пусто = тот же плейсхолдер.")]
        [SerializeField] Sprite buttonSprite;
        [SerializeField] Image.Type buttonImageType = Image.Type.Simple;
        [SerializeField] int titleFontSize = 22;
        [SerializeField] int bodyFontSize = 16;
        [SerializeField] int buttonFontSize = 18;
        [SerializeField] Vector2 bodyMargins = new Vector2(16f, 8f);

        GameObject panelRoot;
        Text bodyText;
        LootContainer currentLoot;
        GameObject currentInteractor;

        void Awake()
        {
            textFont = UiThemeResolver.Font(textFont, theme);

            BuildUi();
            Hide();
        }

        void Update()
        {
            if (panelRoot == null || !panelRoot.activeSelf)
                return;

            if (Input.GetKeyDown(closeKey) || (alternateCloseKey != KeyCode.None && Input.GetKeyDown(alternateCloseKey)))
                Close();
        }

        void BuildUi()
        {
            var canvasGo = new GameObject("LootCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasSortOrder;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            canvasGo.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("LootPanel");
            var panelRt = panelRoot.AddComponent<RectTransform>();
            panelRt.SetParent(canvasGo.transform, false);
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = panelSize;

            var bg = panelRoot.AddComponent<Image>();
            bg.sprite = UiThemeResolver.Sprite(panelBackgroundSprite, theme, theme != null ? theme.lootPanelBackgroundSprite : null);
            bg.type = panelImageType;
            bg.color = panelBackgroundColor;

            var titleGo = new GameObject("Title");
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.SetParent(panelRt, false);
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -12f);
            titleRt.sizeDelta = new Vector2(0f, 36f);
            var title = titleGo.AddComponent<Text>();
            title.font = textFont;
            title.fontSize = titleFontSize;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.text = "Loot";

            var bodyGo = new GameObject("Body");
            var bodyRt = bodyGo.AddComponent<RectTransform>();
            bodyRt.SetParent(panelRt, false);
            bodyRt.anchorMin = new Vector2(0f, 0.25f);
            bodyRt.anchorMax = new Vector2(1f, 0.92f);
            bodyRt.offsetMin = new Vector2(bodyMargins.x, bodyMargins.y);
            bodyRt.offsetMax = new Vector2(-bodyMargins.x, -bodyMargins.y);
            bodyText = bodyGo.AddComponent<Text>();
            bodyText.font = textFont;
            bodyText.fontSize = bodyFontSize;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.color = Color.white;

            CreateButton(panelRt, "TakeAll", "Take all", 0.05f, 0.48f, buttonColor, buttonFontSize, OnTakeAllClicked);
            CreateButton(panelRt, "Close", "Close", 0.52f, 0.95f, buttonColor, buttonFontSize, Close);
        }

        void CreateButton(RectTransform parent, string name, string label, float xMin, float xMax, Color bgColor, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(xMin, 0.05f);
            rt.anchorMax = new Vector2(xMax, 0.2f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.sprite = UiThemeResolver.Sprite(buttonSprite, theme, theme != null ? theme.lootButtonSprite : null);
            img.type = buttonImageType;
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Label");
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var t = textGo.AddComponent<Text>();
            t.font = textFont;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.text = label;
        }

        public void Open(LootContainer loot, GameObject interactor)
        {
            currentLoot = loot;
            currentInteractor = interactor;
            RefreshBody();
            if (panelRoot != null && !panelRoot.activeSelf)
                UiInputBootstrap.PushUiCursor();
            panelRoot.SetActive(true);
        }

        void RefreshBody()
        {
            if (bodyText == null || currentLoot == null)
                return;

            var sb = new StringBuilder();
            var list = currentLoot.Contents;
            for (var i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null || e.item == null || e.quantity <= 0)
                    continue;
                sb.AppendLine($"{e.item.ItemName} x{e.quantity}");
            }

            if (sb.Length == 0)
                sb.Append("(empty)");
            bodyText.text = sb.ToString();
        }

        void OnTakeAllClicked()
        {
            if (currentLoot == null || currentInteractor == null)
                return;
            currentLoot.TryTakeAll(currentInteractor);
            if (currentLoot == null)
            {
                Close();
                return;
            }

            RefreshBody();
            currentLoot.NotifyContentsChanged();
        }

        void Close()
        {
            currentLoot = null;
            currentInteractor = null;
            Hide();
        }

        void Hide()
        {
            if (panelRoot == null)
                return;
            if (panelRoot.activeSelf)
                UiInputBootstrap.PopUiCursor();
            panelRoot.SetActive(false);
        }
    }
}
