using System;
using System.IO;
using Data;
using DataStructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    /// <summary>Displays one shop item and forwards purchase or equip requests to its owner.</summary>
    public class ShopItem : MonoBehaviour
    {
        [SerializeField] private Image _imageComponent;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private TextMeshProUGUI _equipButtonText;

        private ShopItemDataModel _data;
        private bool _hasPurchased;
        private bool _isEquipped;
        private Action<int> _purchaseRequested;
        private Action<int> _equipRequested;

        /// <summary>Whether the item is already owned or has at least one remaining use.</summary>
        public bool HasPurchased => _hasPurchased;
        /// <summary>Whether this theme is currently selected.</summary>
        public bool IsEquipped => _isEquipped;
        /// <summary>Whether the item can be bought under its configured purchase policy.</summary>
        public bool CanPurchase => _data != null && _data.CanPurchase(_hasPurchased);

        // اینجا نیز می‌توانیم همچنان از event tunnel استفاده کنیم به جای callback؛ ولی به نظر می‌رسد چون در یک سیستم هستند چندان مفید نباشد.
        /// <summary>Sets the item's data, rendered state, and action callbacks.</summary>
        public void Initialize(
            ShopItemDataModel data,
            bool hasPurchased,
            bool isEquipped,
            Action<int> purchaseRequested,
            Action<int> equipRequested)
        {
            _data = data;
            _hasPurchased = hasPurchased;
            _isEquipped = isEquipped;
            _purchaseRequested = purchaseRequested;
            _equipRequested = equipRequested;

            var sprite = Resources.Load<Sprite>(Path.Combine(ShopData.SpritesGeneralPath, data.id.ToString()));
            if (_imageComponent != null)
                _imageComponent.sprite = sprite;
            if (_nameText != null)
                _nameText.text = data.name;
            if (_priceText != null)
                _priceText.text = data.price.ToString();

            var isTheme = data.type == ShopItemType.PlayerTheme || data.type == ShopItemType.WorldTheme;
            if (_purchaseButton != null)
            {
                _purchaseButton.gameObject.SetActive(!isTheme || !hasPurchased);
                _purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
                _purchaseButton.onClick.AddListener(HandlePurchaseClicked);
                _purchaseButton.interactable = CanPurchase;
            }

            if (_equipButton != null)
            {
                _equipButton.gameObject.SetActive(isTheme && hasPurchased);
                _equipButton.onClick.RemoveListener(HandleEquipClicked);
                _equipButton.onClick.AddListener(HandleEquipClicked);
                _equipButton.interactable = hasPurchased && !isEquipped;
            }
            if (_equipButtonText != null && isTheme)
                _equipButtonText.text = isEquipped ? "Equipped" : "Equip";
        }

        private void HandlePurchaseClicked()
        {
            if (CanPurchase && _data != null)
                _purchaseRequested?.Invoke(_data.id);
        }

        private void HandleEquipClicked()
        {
            if (_data != null && _hasPurchased && !_isEquipped)
                _equipRequested?.Invoke(_data.id);
        }

        private void OnDestroy()
        {
            if (_purchaseButton != null)
                _purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
            if (_equipButton != null)
                _equipButton.onClick.RemoveListener(HandleEquipClicked);
        }
    }
}
