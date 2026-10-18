using System.Collections.Generic;
using Data;
using Enums;
using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine;

namespace UI.Shop
{
    /// <summary>
    /// Owns the shop screen presentation and forwards player actions to ShopHandler.
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private ShopData _shopData;
        [SerializeField] private ShopItem _shopItemPrefab;
        [SerializeField] private Transform _contentPanel;
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private IntEvent _onTabSelected;
        [SerializeField] private NullEvent _onShopDataLoading;
        [SerializeField] private NullEvent _onShopDataLoaded;

        private readonly List<ShopItem> _spawnedItems = new List<ShopItem>();
        private ShopHandler _shopHandler;

        private void OnEnable()
        {
            if (_onTabSelected != null)
                _onTabSelected.OnEventRaised += HandleTabSelected;
        }

        private void OnDisable()
        {
            if (_onTabSelected != null)
                _onTabSelected.OnEventRaised -= HandleTabSelected;
        }

        public void Refresh()
        {
            if (_onShopDataLoading != null)
                _onShopDataLoading.Raise();

            if (!TryGetHandler())
            {
                ShowMessage("Shop is not configured.");
                FinishLoading();
                return;
            }

            if (_shopItemPrefab == null || _contentPanel == null || _currencyText == null)
            {
                ShowMessage("Shop view references are missing.");
                FinishLoading();
                return;
            }

            if (!_shopData.LoadCatalog())
            {
                ShowMessage("Shop data could not be loaded.");
                FinishLoading();
                return;
            }

            ClearItems();
            _currencyText.text = $"Currency : {_shopHandler.GetCurrency()}";
            ShowMessage(string.Empty);

            foreach (var itemData in _shopData.Items)
            {
                var shopItem = Instantiate(_shopItemPrefab, _contentPanel);
                shopItem.Initialize(
                    itemData,
                    _shopHandler.HasPurchased(itemData.id),
                    _shopHandler.IsEquipped(itemData.id),
                    HandlePurchaseRequested,
                    HandleEquipRequested);
                _spawnedItems.Add(shopItem);
            }

            FinishLoading();
        }

        private void HandleTabSelected(int selectedTab)
        {
            if (selectedTab == (int)TabType.Shop)
                Refresh();
        }

        private void HandlePurchaseRequested(int itemId)
        {
            var result = _shopHandler.TryPurchase(itemId);
            switch (result)
            {
                case ShopPurchaseResult.Success:
                    _currencyText.text = $"Currency : {_shopHandler.GetCurrency()}";
                    RebuildItems();
                    ShowMessage("Purchase complete.");
                    break;
                case ShopPurchaseResult.InsufficientCurrency:
                    ShowMessage("Not enough currency.");
                    break;
                case ShopPurchaseResult.AlreadyOwned:
                    ShowMessage("Already owned.");
                    RebuildItems();
                    break;
                case ShopPurchaseResult.ItemNotFound:
                    ShowMessage("This item is no longer available.");
                    break;
                default:
                    ShowMessage("This item cannot be purchased.");
                    break;
            }
        }

        private void HandleEquipRequested(int themeId)
        {
            var result = _shopHandler.TryEquipTheme(themeId);
            if (result == ShopEquipResult.Success)
            {
                RebuildItems();
                ShowMessage("Theme equipped.");
            }
            else if (result == ShopEquipResult.ThemeNotOwned)
            {
                ShowMessage("Buy this theme before equipping it.");
            }
            else
            {
                ShowMessage("This theme cannot be equipped.");
            }
        }

        private bool TryGetHandler()
        {
            if (_shopHandler != null)
                return true;

            if (_shopData == null || PlayerPrefsSaveService.Main == null)
                return false;

            _shopHandler = new ShopHandler(_shopData, PlayerPrefsSaveService.Main);
            return true;
        }

        private void RebuildItems()
        {
            ClearItems();
            foreach (var itemData in _shopData.Items)
            {
                var shopItem = Instantiate(_shopItemPrefab, _contentPanel);
                shopItem.Initialize(
                    itemData,
                    _shopHandler.HasPurchased(itemData.id),
                    _shopHandler.IsEquipped(itemData.id),
                    HandlePurchaseRequested,
                    HandleEquipRequested);
                _spawnedItems.Add(shopItem);
            }
        }

        private void ClearItems()
        {
            foreach (var shopItem in _spawnedItems)
            {
                if (shopItem != null)
                    Destroy(shopItem.gameObject);
            }
            _spawnedItems.Clear();
        }

        private void ShowMessage(string message)
        {
            if (_messageText != null)
                _messageText.text = message;
        }

        private void FinishLoading()
        {
            if (_onShopDataLoaded != null)
                _onShopDataLoaded.Raise();
        }
    }
}
