using System.Collections.Generic;
using DataStructure;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Loads and exposes the static shop catalog. Player ownership and currency
    /// are stored separately by ShopHandler and PlayerPrefsSaveService.
    /// </summary>
    public class ShopData : MonoBehaviour
    {
        private const string CatalogResourcePath = "Shop/ShopData";

        public const string SpritesGeneralPath = "Shop/Sprites";

        private readonly List<ShopItemDataModel> _items = new List<ShopItemDataModel>();
        private readonly Dictionary<int, ShopItemDataModel> _itemsById = new Dictionary<int, ShopItemDataModel>();

        private bool _loadAttempted;
        private bool _isLoaded;

        public IReadOnlyList<ShopItemDataModel> Items
        {
            get
            {
                LoadCatalog();
                return _items;
            }
        }

        public bool LoadCatalog()
        {
            if (_loadAttempted)
                return _isLoaded;

            _loadAttempted = true;
            var serializedCatalog = Resources.Load<TextAsset>(CatalogResourcePath);
            if (serializedCatalog == null)
            {
                Debug.LogError($"Shop catalog was not found at Resources/{CatalogResourcePath}.json", this);
                return false;
            }

            ShopDataWrapper catalog;
            try
            {
                catalog = JsonUtility.FromJson<ShopDataWrapper>(serializedCatalog.text);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Could not parse the shop catalog: {exception.Message}", this);
                return false;
            }

            if (catalog == null || catalog.items == null)
            {
                Debug.LogError("Shop catalog is empty or has an invalid format.", this);
                return false;
            }

            foreach (var item in catalog.items)
            {
                if (item == null)
                {
                    Debug.LogError("Shop catalog contains an empty item entry.", this);
                    _items.Clear();
                    _itemsById.Clear();
                    return false;
                }

                if (_itemsById.ContainsKey(item.id))
                {
                    Debug.LogError($"Shop catalog contains duplicate item id {item.id}.", this);
                    _items.Clear();
                    _itemsById.Clear();
                    return false;
                }

                _items.Add(item);
                _itemsById.Add(item.id, item);
            }

            _isLoaded = true;
            return true;
        }

        public bool TryGetItem(int itemId, out ShopItemDataModel item)
        {
            if (LoadCatalog() && _itemsById.TryGetValue(itemId, out item))
                return true;

            item = null;
            return false;
        }

        public static bool TryLoadItemFromResources(int itemId, out ShopItemDataModel item)
        {
            item = null;
            if (!TryLoadItemsFromResources(out var items))
                return false;

            item = items.Find(candidate => candidate != null && candidate.id == itemId);
            return item != null;
        }

        public static bool TryLoadItemsFromResources(out List<ShopItemDataModel> items)
        {
            items = null;
            var serializedCatalog = Resources.Load<TextAsset>(CatalogResourcePath);
            if (serializedCatalog == null)
            {
                Debug.LogError($"Shop catalog was not found at Resources/{CatalogResourcePath}.json");
                return false;
            }

            try
            {
                var catalog = JsonUtility.FromJson<ShopDataWrapper>(serializedCatalog.text);
                if (catalog?.items == null)
                    return false;

                items = catalog.items;
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Could not parse the shop catalog: {exception.Message}");
            }

            return false;
        }
    }
}
