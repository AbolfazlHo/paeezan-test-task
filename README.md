# Paeezan — Unity Test Project

This repository contains a Unity game project with a splash-to-game scene flow, score-based currency, a data-driven shop, selectable themes, and consumable powerups.

## Documentation

- [Architecture overview](ARCHITECTURE.md)
- [Persian README](README_PERSIAN.md)
- [Persian architecture overview](ARCHITECTURE_PERSIAN.md)

## Run the project

1. Open the repository root in Unity Hub. The project records **Unity 6000.0.30f1** in `ProjectSettings/ProjectVersion.txt`. The evaluation brief specifies **Unity 6000.0.58f2**; use that editor for evaluation and let Unity reimport the project if prompted.
2. Open `Assets/Scenes/Scene_Splash.unity` and press **Play**. The enabled Build Settings sequence is Splash → Loading → Menu → Game. `BootStrap.unity` is disabled in Build Settings.
3. Edit `Assets/Resources/Shop/ShopData.json` to change shop entries, prices, icons, purchase policies, or powerup effects.

## Gameplay and shop features

### Currency

At run end, `BallController.EarnCurrency` converts the score to currency using integer division by 10, adds it to the saved `Currency` balance, and persists the value through `PlayerPrefsSaveService.SaveInt`. Scores below 10 award zero; zero or negative scores exit without saving. `BallController.OnEnable` subscribes the method to the `OnGameEnded` `NullEvent`. After saving, it raises `OnCurrencyEarned` with the new balance. No listener for that event is currently registered. The shop reads the balance through `ShopHandler.GetCurrency` when `ShopView.Refresh` runs.

### Catalog, purchases, and themes

`ShopData` loads `Assets/Resources/Shop/ShopData.json` as a `TextAsset`, parses `ShopItemDataModel` entries, and indexes them by ID. A missing or invalid catalog and duplicate IDs are rejected. The catalog defines item type, price, purchase policy, icon ID, and powerup effect; the UI builds its cards from these entries.

When the shop is selected, `ShopView.Refresh` creates a `ShopHandler` and a `ShopItem` view for each catalog entry. Each item loads its icon by item ID and forwards button actions to the view. `ShopHandler.TryPurchase` validates the item and policy, prevents repurchasing unique themes, and checks the currency balance. A successful theme purchase adds its ID to `ownedThemeIds`; a powerup purchase increments its inventory. `PlayerPrefsSaveService.SaveShopPurchase` saves progress and currency together, then the shop rebuilds its cards.

Player and world themes are equipped independently. `ShopHandler.TryEquipTheme` only equips an owned theme. At gameplay startup, `ShopThemeMaterialApplier` checks the selected ID against the catalog, ownership, and configured `ShopThemeRole`, then loads `Resources/Shop/Materials/{id}`. If no theme is selected (`-1`) or validation/loading fails, the renderer keeps its default material.

### Powerups and persistence

`PowerupHudController` creates a HUD button for every catalog entry of type `Powerup`; each label shows the catalog name and saved count. The controller listens to the configured run-start and run-end `NullEvent` channels. A powerup can be used only during an active run, when its count is positive and its effect is available.

`ShieldNextHit` first asks `BallController.TryActivateShield` to arm a one-hit shield, then consumes one use. When the shield absorbs an obstacle hit, `BallController` clears it and raises the C# `ActiveShieldConsumed` event so the HUD can refresh. `SlowMotion` consumes one use and starts a timed coroutine. The run-end handler stops the effect and restores `Time.timeScale`.

After activation, the HUD decrements the saved count and calls `PlayerPrefsSaveService.SaveShopProgress`. `ShopProgressData` stores owned theme IDs, selected player/world theme IDs, and powerup inventory (`powerupId` and `count`) as JSON under the `ShopProgress` PlayerPrefs key. `Normalize` repairs missing collections, removes duplicate theme IDs, and merges duplicate positive inventory entries. Currency is stored separately under the integer `Currency` key. Each save also refreshes the player-data JSON export through `PlayerDataJsonExporter`.

