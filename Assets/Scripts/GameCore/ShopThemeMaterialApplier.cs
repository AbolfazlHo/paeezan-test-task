using System.Collections.Generic;
using Data;
using DataStructure;
using ScriptableObjects.Services;
using UnityEngine;

namespace GameCore
{
    public enum ShopThemeRole
    {
        Player,
        World
    }

    /// <summary>
    /// Applies the persisted player or world theme material when the game scene loads.
    /// A selected ID of -1 leaves the prefab's default appearance unchanged.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShopThemeMaterialApplier : MonoBehaviour
    {
        private const string MaterialsResourcePath = "Shop/Materials";

        [SerializeField] private ShopThemeRole _role;

        private void Awake()
        {
            var saveService = PlayerPrefsSaveService.Main;
            if (saveService == null)
            {
                Debug.LogWarning("Player preferences service is unavailable; using the default theme.", this);
                return;
            }

            var progress = saveService.LoadShopProgress();
            var selectedThemeId = _role == ShopThemeRole.Player
                ? progress.selectedPlayerThemeId
                : progress.selectedWorldThemeId;
            if (selectedThemeId == -1)
                return;

            if (!ShopData.TryLoadItemFromResources(selectedThemeId, out var item) || !IsExpectedTheme(item))
            {
                Debug.LogWarning($"Selected {_role.ToString().ToLowerInvariant()} theme {selectedThemeId} is missing or has the wrong type; using the default theme.", this);
                return;
            }

            if (!progress.ownedThemeIds.Contains(selectedThemeId))
            {
                Debug.LogWarning($"Selected theme {selectedThemeId} is not owned; using the default theme.", this);
                return;
            }

            var materialResourcePath = $"{MaterialsResourcePath}/{selectedThemeId}";
            var themeMaterial = Resources.Load<Material>(materialResourcePath);
            if (themeMaterial == null)
            {
                Debug.LogWarning($"Theme material was not found at Resources/{materialResourcePath}.mat; using the default theme.", this);
                return;
            }

            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sharedMaterial = themeMaterial;
            spriteRenderer.color = Color.white;
        }

        private bool IsExpectedTheme(ShopItemDataModel item)
        {
            if (item == null)
                return false;

            return _role == ShopThemeRole.Player
                ? item.type == ShopItemType.PlayerTheme
                : item.type == ShopItemType.WorldTheme;
        }
    }
}
