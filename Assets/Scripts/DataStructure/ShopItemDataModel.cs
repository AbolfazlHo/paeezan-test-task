using System;
using JetBrains.Annotations;

namespace DataStructure
{
    public enum ShopItemType
    {
        PlayerTheme = 1,
        WorldTheme = 2,
        Powerup = 3
    }

    public enum ShopItemPurchasePolicy
    {
        Unspecified = 0,
        UniqueUnlock = 1,
        Stackable = 2
    }

    public enum ShopPowerupEffect
    {
        None = 0,
        ShieldNextHit = 1,
        SlowMotion = 2
    }
    
    [Serializable]
    public class ShopItemDataModel
    {
//        public int Id { get; set; }
//        public ShopItemType Type { get; set; }
//        public string Name { get; set; }
//        public int Price { get; set; }


        public int id;
        public ShopItemType type;
        public ShopItemPurchasePolicy purchasePolicy;
        public string name;
        public int price;
        public ShopPowerupEffect powerupEffect;

        public ShopItemPurchasePolicy GetPurchasePolicy()
        {
            if (purchasePolicy != ShopItemPurchasePolicy.Unspecified)
                return purchasePolicy;

            return type == ShopItemType.Powerup
                ? ShopItemPurchasePolicy.Stackable
                : ShopItemPurchasePolicy.UniqueUnlock;
        }

        public bool CanPurchase(bool alreadyOwned)
        {
            return GetPurchasePolicy() == ShopItemPurchasePolicy.Stackable || !alreadyOwned;
        }
    }
}
