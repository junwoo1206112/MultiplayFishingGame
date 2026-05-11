using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MultiplayFishing.Core;
using MultiplayFishing.Data.Models;

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
        public string ItemId => itemId;

        private InventoryItem itemData;
        private FishDataSO fishInfo;
        private string itemId;
        private Action<InventorySlotUI> buttonAction;

        public event Action<InventorySlotUI> OnSlotClicked;

        private void Awake()
        {
            AutoBindReferences();
        }

        public void SetupInventoryFish(InventoryItem item, FishDataSO data, IUserService userService)
        {
            Setup(
                item != null ? item.fishId : string.Empty,
                data != null ? data.fishName : "Unknown",
                data != null ? data.fishIcon : null,
                item != null ? $"{item.length:F1} cm" : string.Empty,
                "판매",
                () => userService?.SellFish(item.instanceId));

            itemData = item;
            fishInfo = data;
        }

        public void SetupFishCatalog(FishDataSO data, Action<InventorySlotUI> onBuy)
        {
            Setup(
                data != null ? data.id : string.Empty,
                data != null ? data.fishName : "Unknown",
                data != null ? data.fishIcon : null,
                data != null ? $"{data.sellPrice:N0} G" : string.Empty,
                data != null ? $"{data.sellPrice:N0} G" : string.Empty,
                () => onBuy?.Invoke(this));

            fishInfo = data;
        }

        public void SetupRod(RodDataSO data, bool owned, bool equipped, Action<InventorySlotUI> onAction)
        {
            string actionText = equipped ? "해제" : owned ? "장착" : data != null ? $"{data.price:N0} G" : string.Empty;
            Setup(
                data != null ? data.id : string.Empty,
                data != null ? data.rodName : "Unknown",
                data != null ? data.icon : null,
                data != null ? data.rank : string.Empty,
                actionText,
                () => onAction?.Invoke(this));
        }

        public void SetupBait(BaitDataSO data, bool owned, bool equipped, Action<InventorySlotUI> onAction)
        {
            string actionText = equipped ? "해제" : owned ? "장착" : data != null ? $"{data.price:N0} G" : string.Empty;
            Setup(
                data != null ? data.id : string.Empty,
                data != null ? data.baitName : "Unknown",
                data != null ? data.icon : null,
                data != null ? data.rank : string.Empty,
                actionText,
                () => onAction?.Invoke(this));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(this);
            }
        }

        private void Setup(string id, string displayName, Sprite icon, string subText, string actionText, Action action)
        {
            AutoBindReferences();

            itemData = null;
            fishInfo = null;
            itemId = id;
            buttonAction = _ => action?.Invoke();

            if (nameText != null) nameText.text = displayName;
            if (lengthText != null) lengthText.text = subText;

            if (fishIcon != null)
            {
                fishIcon.sprite = icon != null ? icon : CreatePlaceholderSprite();
                fishIcon.preserveAspect = true;
            }

            if (sellButton == null) return;

            TMP_Text buttonText = sellButton.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null) buttonText.text = actionText;

            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => buttonAction?.Invoke(this));
            sellButton.gameObject.SetActive(!string.IsNullOrEmpty(actionText));
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

        private static Sprite CreatePlaceholderSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 64;
                int y = i / 64;
                pixels[i] = (x / 8 + y / 8) % 2 == 0
                    ? new Color(0.3f, 0.3f, 0.3f)
                    : new Color(0.5f, 0.5f, 0.5f);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100);
            sprite.name = "PlaceholderIcon";
            return sprite;
        }
    }
}
