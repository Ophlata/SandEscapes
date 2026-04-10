using SandEscapes.Inventory;
using SandEscapes.Items;
using UnityEngine;
using UnityEngine.UI;

namespace SandEscapes.UI
{
    /// <summary>
    /// Full inventory grid toggle (Tab). Minimal icons + stack counts.
    /// Sizes/colors below are read when the panel is built at Start (change in Inspector, then Play).
    /// </summary>
    public class InventoryPanelUIView : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] GameUiTheme theme;

        [SerializeField] InventorySystem inventory;
        [SerializeField] KeyCode toggleKey = KeyCode.Tab;

        [Header("Grid layout")]
        [SerializeField] Vector2 cellSize = new Vector2(48f, 48f);
        [SerializeField] int columns = 6;
        [SerializeField] float panelPadding = 16f;
        [SerializeField] Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] int canvasSortOrder = 180;

        [Header("Custom graphics (optional, override theme)")]
        [Tooltip("Фон панели инвентаря. Пусто = плейсхолдер + тинт.")]
        [SerializeField] Sprite panelBackgroundSprite;
        [SerializeField] Image.Type panelImageType = Image.Type.Simple;
        [Tooltip("Рамка каждой ячейки. Пусто = плейсхолдер.")]
        [SerializeField] Sprite cellFrameSprite;
        [SerializeField] Image.Type cellImageType = Image.Type.Simple;
        [SerializeField] Color panelBackgroundColor = new Color(0.06f, 0.06f, 0.08f, 0.94f);
        [SerializeField] Color cellFrameColor = new Color(0.2f, 0.2f, 0.22f, 1f);
        [SerializeField] Font textFont;

        GameObject panelRoot;
        Text[] quantityLabels;
        Image[] iconImages;
        bool built;

        void Reset()
        {
            inventory = GetComponent<InventorySystem>();
        }

        void Awake()
        {
            if (inventory == null)
                inventory = GetComponent<InventorySystem>();

            textFont = UiThemeResolver.Font(textFont, theme);
        }

        void Start()
        {
            if (!built && inventory != null)
            {
                BuildPanel();
                built = true;
            }

            Hide();
        }

        void OnEnable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged += Refresh;
        }

        void OnDisable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= Refresh;
        }

        void Update()
        {
            if (panelRoot == null)
                return;
            if (Input.GetKeyDown(toggleKey))
            {
                if (panelRoot.activeSelf)
                    Hide();
                else
                    Show();
            }
        }

        void BuildPanel()
        {
            if (inventory == null || inventory.TotalSlotCount <= 0)
                return;

            var canvasGo = new GameObject("InventoryCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvasSortOrder;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            canvasGo.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("InventoryPanel");
            var panelRt = panelRoot.AddComponent<RectTransform>();
            panelRt.SetParent(canvasGo.transform, false);
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);

            var rows = Mathf.CeilToInt(inventory.TotalSlotCount / (float)columns);
            var w = columns * cellSize.x + panelPadding * 2f;
            var h = rows * cellSize.y + panelPadding * 2f + 40f;
            panelRt.sizeDelta = new Vector2(w, h);

            var bg = panelRoot.AddComponent<Image>();
            bg.sprite = UiThemeResolver.Sprite(panelBackgroundSprite, theme, theme != null ? theme.inventoryPanelBackgroundSprite : null);
            bg.type = panelImageType;
            bg.color = panelBackgroundColor;

            var titleGo = new GameObject("Title");
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.SetParent(panelRt, false);
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -8f);
            titleRt.sizeDelta = new Vector2(0f, 28f);
            var title = titleGo.AddComponent<Text>();
            title.font = textFont;
            title.fontSize = 20;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.text = "Inventory (Tab)";

            var gridGo = new GameObject("Grid");
            var gridRt = gridGo.AddComponent<RectTransform>();
            gridRt.SetParent(panelRt, false);
            gridRt.anchorMin = new Vector2(0f, 0f);
            gridRt.anchorMax = new Vector2(1f, 1f);
            gridRt.offsetMin = new Vector2(panelPadding, panelPadding);
            gridRt.offsetMax = new Vector2(-panelPadding, -40f);

            var total = inventory.TotalSlotCount;
            quantityLabels = new Text[total];
            iconImages = new Image[total];

            for (var i = 0; i < total; i++)
            {
                var col = i % columns;
                var row = i / columns;

                var cell = new GameObject($"Slot_{i}");
                var cellRt = cell.AddComponent<RectTransform>();
                cellRt.SetParent(gridRt, false);
                cellRt.anchorMin = new Vector2(0f, 1f);
                cellRt.anchorMax = new Vector2(0f, 1f);
                cellRt.pivot = new Vector2(0f, 1f);
                cellRt.anchoredPosition = new Vector2(col * cellSize.x, -row * cellSize.y);
                cellRt.sizeDelta = cellSize;

                var frame = cell.AddComponent<Image>();
                frame.sprite = UiThemeResolver.Sprite(cellFrameSprite, theme, theme != null ? theme.inventoryCellFrameSprite : null);
                frame.type = cellImageType;
                frame.color = cellFrameColor;

                var iconGo = new GameObject("Icon");
                var iconRt = iconGo.AddComponent<RectTransform>();
                iconRt.SetParent(cellRt, false);
                iconRt.anchorMin = new Vector2(0.1f, 0.15f);
                iconRt.anchorMax = new Vector2(0.9f, 0.85f);
                iconRt.offsetMin = Vector2.zero;
                iconRt.offsetMax = Vector2.zero;
                var icon = iconGo.AddComponent<Image>();
                icon.sprite = UiThemeResolver.Sprite(null, theme, theme != null ? theme.hotbarIconPlaceholderSprite : null);
                icon.preserveAspect = true;
                icon.enabled = false;
                iconImages[i] = icon;

                var qtyGo = new GameObject("Qty");
                var qtyRt = qtyGo.AddComponent<RectTransform>();
                qtyRt.SetParent(cellRt, false);
                qtyRt.anchorMin = new Vector2(1f, 0f);
                qtyRt.anchorMax = new Vector2(1f, 0f);
                qtyRt.pivot = new Vector2(1f, 0f);
                qtyRt.anchoredPosition = new Vector2(-2f, 2f);
                qtyRt.sizeDelta = new Vector2(36f, 20f);
                var qty = qtyGo.AddComponent<Text>();
                qty.font = textFont;
                qty.fontSize = 14;
                qty.alignment = TextAnchor.LowerRight;
                qty.color = Color.white;
                quantityLabels[i] = qty;
            }
        }

        void Show()
        {
            if (panelRoot == null)
                return;
            if (!panelRoot.activeSelf)
                UiInputBootstrap.PushUiCursor();
            panelRoot.SetActive(true);
            Refresh();
        }

        void Hide()
        {
            if (panelRoot == null)
                return;
            if (panelRoot.activeSelf)
                UiInputBootstrap.PopUiCursor();
            panelRoot.SetActive(false);
        }

        void Refresh()
        {
            if (inventory == null || iconImages == null)
                return;

            for (var i = 0; i < iconImages.Length; i++)
            {
                var slot = inventory.GetSlot(i);
                var icon = iconImages[i];
                var qty = quantityLabels[i];
                if (slot.IsEmpty || slot.Item == null)
                {
                    icon.enabled = false;
                    qty.gameObject.SetActive(false);
                    continue;
                }

                icon.enabled = true;
                icon.sprite = slot.Item.Icon != null ? slot.Item.Icon : UiThemeResolver.Sprite(null, theme, theme != null ? theme.hotbarIconPlaceholderSprite : null);
                var showQty = slot.Quantity > 1;
                qty.gameObject.SetActive(showQty);
                if (showQty)
                    qty.text = slot.Quantity.ToString();
            }
        }
    }
}
