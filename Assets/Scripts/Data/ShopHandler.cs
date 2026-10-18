using System;
using DataStructure;
using ScriptableObjects.Services;

namespace Data
{
    public enum ShopPurchaseResult
    {
        Success,
        ItemNotFound,
        InvalidPrice,
        AlreadyOwned,
        InsufficientCurrency,
        InventoryFull,
        InvalidPurchasePolicy
    }

    public enum ShopEquipResult
    {
        Success,
        ItemNotFound,
        NotATheme,
        ThemeNotOwned
    }

    /// <summary>
    /// Applies shop rules and persists player-owned state. UI components should
    /// call this handler with item ids instead of changing currency or inventory.
    /// </summary>
    public class ShopHandler
    {
        private readonly ShopData _shopData;
        private readonly PlayerPrefsSaveService _saveService;

        public ShopHandler(ShopData shopData, PlayerPrefsSaveService saveService)
        {
            if (shopData == null)
                throw new ArgumentNullException(nameof(shopData));
            if (saveService == null)
                throw new ArgumentNullException(nameof(saveService));

            _shopData = shopData;
            _saveService = saveService;
        }

        public int GetCurrency()
        {
            return _saveService.LoadInt("Currency");
        }

        public ShopProgressData LoadProgress()
        {
            return _saveService.LoadShopProgress();
        }

        public bool HasPurchased(int itemId)
        {
            if (!_shopData.TryGetItem(itemId, out var item))
                return false;

            var progress = _saveService.LoadShopProgress();
            if (item.type == ShopItemType.Powerup)
                return progress.GetPowerupCount(itemId) > 0;

            return progress.ownedThemeIds.Contains(itemId);
        }

        public bool IsEquipped(int itemId)
        {
            if (!_shopData.TryGetItem(itemId, out var item))
                return false;

            var progress = _saveService.LoadShopProgress();
            if (item.type == ShopItemType.PlayerTheme)
                return progress.selectedPlayerThemeId == itemId;
            if (item.type == ShopItemType.WorldTheme)
                return progress.selectedWorldThemeId == itemId;
            return false;
        }

        public ShopPurchaseResult TryPurchase(int itemId)
        {
            if (!_shopData.TryGetItem(itemId, out var item))
                return ShopPurchaseResult.ItemNotFound;
            if (item.price < 0)
                return ShopPurchaseResult.InvalidPrice;

            var policy = item.GetPurchasePolicy();
            var isTheme = item.type == ShopItemType.PlayerTheme || item.type == ShopItemType.WorldTheme;
            if ((policy != ShopItemPurchasePolicy.UniqueUnlock && policy != ShopItemPurchasePolicy.Stackable) ||
                (policy == ShopItemPurchasePolicy.UniqueUnlock && !isTheme) ||
                (policy == ShopItemPurchasePolicy.Stackable && item.type != ShopItemType.Powerup))
                return ShopPurchaseResult.InvalidPurchasePolicy;

            var progress = _saveService.LoadShopProgress();
            var alreadyOwned = isTheme && progress.ownedThemeIds.Contains(itemId);
            if (!item.CanPurchase(alreadyOwned))
                return ShopPurchaseResult.AlreadyOwned;

            var currency = _saveService.LoadInt("Currency");
            if (currency < item.price)
                return ShopPurchaseResult.InsufficientCurrency;

            if (policy == ShopItemPurchasePolicy.UniqueUnlock)
            {
                progress.ownedThemeIds.Add(itemId);
            }
            else
            {
                var currentCount = progress.GetPowerupCount(itemId);
                if (currentCount == int.MaxValue)
                    return ShopPurchaseResult.InventoryFull;

                progress.SetPowerupCount(itemId, currentCount + 1);
            }

            _saveService.SaveShopPurchase(progress, currency - item.price);
            return ShopPurchaseResult.Success;
        }

        public ShopEquipResult TryEquipTheme(int themeId)
        {
            if (!_shopData.TryGetItem(themeId, out var item))
                return ShopEquipResult.ItemNotFound;

            var progress = _saveService.LoadShopProgress();
            if (item.type == ShopItemType.PlayerTheme)
            {
                if (!progress.ownedThemeIds.Contains(themeId))
                    return ShopEquipResult.ThemeNotOwned;

                progress.selectedPlayerThemeId = themeId;
            }
            else if (item.type == ShopItemType.WorldTheme)
            {
                if (!progress.ownedThemeIds.Contains(themeId))
                    return ShopEquipResult.ThemeNotOwned;

                progress.selectedWorldThemeId = themeId;
            }
            else
            {
                return ShopEquipResult.NotATheme;
            }

            _saveService.SaveShopProgress(progress);
            return ShopEquipResult.Success;
        }
    }
}
