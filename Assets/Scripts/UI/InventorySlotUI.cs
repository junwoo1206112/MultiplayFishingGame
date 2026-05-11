using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using MultiplayFishing.Data.Models;
using MultiplayFishing.Core;

namespace MultiplayFishing.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Image fishIcon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text lengthText;
        [SerializeField] private Button sellButton;

        public InventoryItem ItemData => itemData;
        public FishDataSO FishInfo => fishInfo;

        private InventoryItem itemData;
        private FishDataSO fishInfo;
        private IUserService userService;

        public event System.Action<InventorySlotUI> OnSlotClicked;

        private void Awake()
        {
            AutoBindReferences();
        }

        private void AutoBindReferences()
        {
            if (fishIcon == null)
            {
                Transform icon = transform.Find("FishIcon");
                if (icon != null) fishIcon = icon.GetComponent<Image>();
            }

            if (nameText == null)
            {
                Transform text = transform.Find("NameText");
                if (text != null) nameText = text.GetComponent<TMP_Text>();
            }
            ApplyKoreanFont(nameText);

            if (lengthText == null)
            {
                Transform text = transform.Find("LengthText");
                if (text != null) lengthText = text.GetComponent<TMP_Text>();
            }
            ApplyKoreanFont(lengthText);

            if (sellButton == null)
            {
                Transform button = transform.Find("SellButton");
                if (button != null) sellButton = button.GetComponent<Button>();
            }
        }

        private static void ApplyKoreanFont(TMP_Text label)
        {
            if (label == null || TMP_Settings.fallbackFontAssets == null) return;

            foreach (TMP_FontAsset fontAsset in TMP_Settings.fallbackFontAssets)
            {
                if (fontAsset != null && fontAsset.name.Contains("Nanum"))
                {
                    label.font = fontAsset;
                    return;
                }
            }
        }

        public void Setup(InventoryItem item, FishDataSO fishInfo, IUserService userService)
        {
            AutoBindReferences();

            this.itemData = item;
            this.fishInfo = fishInfo;
            this.userService = userService;

            if (fishInfo != null)
            {
                if (nameText != null) nameText.text = fishInfo.fishName;
                if (fishIcon != null)
                {
                    fishIcon.sprite = fishInfo.fishIcon;
                    if (fishInfo.fishIcon == null)
                    {
                        Debug.LogWarning($"[InventorySlotUI] Fish '{fishInfo.fishName}' (id: {fishInfo.id}) has no icon sprite.");
                        fishIcon.sprite = CreatePlaceholderSprite();
                    }
                }
            }
            else
            {
                if (fishIcon != null) fishIcon.sprite = CreatePlaceholderSprite();
            }

            if (lengthText != null) lengthText.text = $"{item.length:F1} cm";

            if (sellButton != null) sellButton.gameObject.SetActive(true);

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }
        }

        public void SetupCatalog(FishDataSO fishInfo, System.Action<InventorySlotUI> onBuy)
        {
            AutoBindReferences();

            this.itemData = null;
            this.fishInfo = fishInfo;
            this.userService = null;

            if (nameText != null) nameText.text = fishInfo != null ? fishInfo.fishName : "???";
            if (fishIcon != null)
            {
                fishIcon.sprite = fishInfo != null && fishInfo.fishIcon != null ? fishInfo.fishIcon : CreatePlaceholderSprite();
            }
            if (lengthText != null) lengthText.text = fishInfo != null ? $"{fishInfo.sellPrice}G" : "";

            if (sellButton != null)
            {
                var btnText = sellButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null) btnText.text = fishInfo != null ? $"{fishInfo.sellPrice}G" : "";
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(() => onBuy?.Invoke(this));
                sellButton.gameObject.SetActive(true);
            }

            OnSlotClicked += onBuy;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(this);
            }
        }

        private void OnSellClicked()
        {
            userService.SellFish(itemData.instanceId);
        }

        private static Sprite CreatePlaceholderSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                pixels[i] = (x / 8 + y / 8) % 2 == 0 ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
            sprite.name = "PlaceholderIcon";
            return sprite;
        }
    }
}
