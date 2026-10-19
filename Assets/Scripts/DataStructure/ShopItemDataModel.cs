using System;

namespace DataStructure
{
    /// <summary>Classifies catalog entries displayed in the shop.</summary>
    public enum ShopItemType
    {
        /// <summary>Visual theme applied to the player character.</summary>
        PlayerTheme = 1,
        /// <summary>Visual theme applied to the game world.</summary>
        WorldTheme = 2,
        /// <summary>Consumable gameplay effect.</summary>
        Powerup = 3
    }

    /// <summary>Defines whether catalog items can be bought once or repeatedly.</summary>
    public enum ShopItemPurchasePolicy
    {
        /// <summary>Use the policy inferred from the item type.</summary>
        Unspecified = 0,
        /// <summary>Unlock an item once.</summary>
        UniqueUnlock = 1,
        /// <summary>Add one use to a repeatable inventory.</summary>
        Stackable = 2
    }

    /// <summary>Identifies the in-run behavior provided by a powerup.</summary>
    public enum ShopPowerupEffect
    {
        /// <summary>No gameplay effect is configured.</summary>
        None = 0,
        /// <summary>Absorbs the next obstacle hit.</summary>
        ShieldNextHit = 1,
        /// <summary>Temporarily slows game time.</summary>
        SlowMotion = 2
    }
    
    /// <summary>Stores the static type, price, and effect settings for one shop item.</summary>
    [Serializable]
    public class ShopItemDataModel
    {
        /// <summary>Stable identifier used to look up this item and its assets.</summary>
        public int id;
        /// <summary>Determines whether this entry is a player theme, world theme, or powerup.</summary>
        public ShopItemType type;
        /// <summary>Controls whether purchases are unique unlocks or stackable quantities.</summary>
        public ShopItemPurchasePolicy purchasePolicy;
        /// <summary>Name presented to the player.</summary>
        public string name;
        /// <summary>Currency required for one purchase.</summary>
        public int price;
        /// <summary>Gameplay effect used when this entry is a powerup.</summary>
        public ShopPowerupEffect powerupEffect;

        /// <summary>Returns the configured policy, defaulting themes to unique and powerups to stackable.</summary>
        public ShopItemPurchasePolicy GetPurchasePolicy()
        {
            if (purchasePolicy != ShopItemPurchasePolicy.Unspecified)
                return purchasePolicy;

            return type == ShopItemType.Powerup
                ? ShopItemPurchasePolicy.Stackable
                : ShopItemPurchasePolicy.UniqueUnlock;
        }

        /// <summary>Determines whether this item may be bought under the supplied ownership state.</summary>
        public bool CanPurchase(bool alreadyOwned)
        {
            return GetPurchasePolicy() == ShopItemPurchasePolicy.Stackable || !alreadyOwned;
        }
    }
}
