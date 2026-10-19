# Architecture

[فارسی](ARCHITECTURE_PERSIAN.md) · [Project README](README.md)

## Runtime layers

The project separates gameplay behavior, presentation, shop rules, and persistence across components. Build Settings runs Splash → Loading → Menu → Game; `BootStrap.unity` is disabled. Scripts under `GameCore` implement gameplay, while UI scripts display state and forward player actions.

ScriptableObject event channels (`NullEvent` and `IntEvent`) let systems communicate without direct references. The ball publishes score, run-start, and run-end signals; UI, animation, audio, and obstacle systems subscribe as needed. Channel callbacks run synchronously, so these assets are signals rather than a queued event bus. Use them for cross-system notifications. Keep local interactions as direct calls or ordinary C# events where that makes control flow clearer.

## Shop data and rules

`ShopData` reads the static catalog from `Assets/Resources/Shop/ShopData.json`, parses it into `ShopItemDataModel` entries, and indexes entries by ID. The catalog is the source for item types, prices, purchase policies, icon IDs, and powerup effects.

`ShopView` creates shop presentation from the catalog, and `ShopItem` forwards card actions. `ShopHandler` validates purchases and equips, including balance, item-policy compatibility, unique theme ownership, and ownership before equipping. Successful purchases are persisted by `PlayerPrefsSaveService`.

Themes are equipped separately for the player and world. At gameplay startup, `ShopThemeMaterialApplier` validates the selected catalog entry, ownership, and configured role before loading `Resources/Shop/Materials/{id}`. Invalid or absent selections leave the renderer's default material in place.

## Powerups and saved data

`PowerupHudController` builds controls from powerup catalog entries and reads their saved counts. It permits use during an active run when the count is positive and the effect is available. The shield effect is armed by `BallController`; timed slow motion is managed by the HUD controller and reset at run end.

`ShopProgressData` stores owned theme IDs, selected player and world theme IDs, and powerup quantities. It is serialized as JSON under the `ShopProgress` PlayerPrefs key. Currency is stored separately under `Currency`. `Normalize` repairs missing collections, removes duplicate theme IDs, and merges duplicate positive inventory entries. `PlayerPrefsSaveService` handles persistence and refreshes the player-data JSON export through `PlayerDataJsonExporter`.

## Scene flow

The enabled Build Settings sequence is Splash → Loading → Menu → Game. The project version is recorded as Unity 6000.0.30f1; see the README for the evaluation editor version specified by the task brief.
