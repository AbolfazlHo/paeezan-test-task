using System;
using DataStructure;
using ScriptableObjects.Services;

namespace Data
{
    /// <summary>Describes the result of attempting to purchase a catalog item.</summary>
    public enum ShopPurchaseResult
    {
        /// <summary>The purchase was completed.</summary>
        Success,
        /// <summary>The id does not exist in the catalog.</summary>
        ItemNotFound,
        /// <summary>The catalog price is negative.</summary>
        InvalidPrice,
        /// <summary>The player already owns a unique item.</summary>
        AlreadyOwned,
        /// <summary>The player does not have enough currency.</summary>
        InsufficientCurrency,
        /// <summary>The stackable inventory has reached its maximum count.</summary>
        InventoryFull,
        /// <summary>The item type and purchase policy are incompatible.</summary>
        InvalidPurchasePolicy
    }

    /// <summary>Describes the result of equipping a theme.</summary>
    public enum ShopEquipResult
    {
        /// <summary>The theme was selected.</summary>
        Success,
        /// <summary>The id does not exist in the catalog.</summary>
        ItemNotFound,
        /// <summary>The catalog item is not a theme.</summary>
        NotATheme,
        /// <summary>The player has not purchased this theme.</summary>
        ThemeNotOwned
    }

    /// <summary>Applies shop rules and persists player-owned state.</summary>
    public class ShopHandler
    {
        private readonly ShopData _shopData;
        private readonly PlayerPrefsSaveService _saveService;

        /// <summary>Creates a shop handler using the catalog and persistence service.</summary>
        public ShopHandler(ShopData shopData, PlayerPrefsSaveService saveService)
        {
            if (shopData == null)
                throw new ArgumentNullException(nameof(shopData));
            if (saveService == null)
                throw new ArgumentNullException(nameof(saveService));

            _shopData = shopData;
            _saveService = saveService;
        }

        /// <summary>Returns the player's current currency balance.</summary>
        public int GetCurrency()
        {
            return _saveService.LoadInt("Currency");
        }

        /// <summary>Loads the saved themes and powerup inventory.</summary>
        public ShopProgressData LoadProgress()
        {
            return _saveService.LoadShopProgress();
        }

        /// <summary>Returns whether the player owns the theme or has stock of the powerup.</summary>
        public bool HasPurchased(int itemId)
        {
            if (!_shopData.TryGetItem(itemId, out var item))
                return false;

            var progress = _saveService.LoadShopProgress();
            if (item.type == ShopItemType.Powerup)
                return progress.GetPowerupCount(itemId) > 0;

            return progress.ownedThemeIds.Contains(itemId);
        }

        /// <summary>Returns whether a theme is currently selected.</summary>
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

        /// <summary>Validates and applies a purchase, saving its currency and inventory changes.</summary>
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

        /// <summary>Selects an owned player or world theme and saves the selection.</summary>
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
