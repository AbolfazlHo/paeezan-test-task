using System.Collections;
using System.Collections.Generic;
using Data;
using DataStructure;
using GameCore;
using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>Builds powerup controls from the catalog and applies saved inventory during a run.</summary>
    public class PowerupHudController : MonoBehaviour
    {
        [SerializeField] private NullEvent _onRunStarted;
        [SerializeField] private NullEvent _onRunEnded;
        [SerializeField] private float _slowMotionScale = 0.5f;
        [SerializeField] private float _slowMotionDuration = 5f;

        private readonly List<PowerupButton> _buttons = new List<PowerupButton>();
        private BallController _ballController;
        private PlayerPrefsSaveService _saveService;
        private Coroutine _slowMotionCoroutine;
        private bool _runIsActive;

        private void Awake()
        {
            _ballController = FindFirstObjectByType<BallController>();
            if (_ballController != null)
                _ballController.ActiveShieldConsumed += RefreshButtons;

            _saveService = PlayerPrefsSaveService.Main;
            CreateButtonsFromCatalog();
            RefreshButtons();
        }

        private void OnEnable()
        {
            if (_onRunStarted != null)
                _onRunStarted.OnEventRaised += HandleRunStarted;
            if (_onRunEnded != null)
                _onRunEnded.OnEventRaised += HandleRunEnded;
        }

        private void OnDisable()
        {
            if (_onRunStarted != null)
                _onRunStarted.OnEventRaised -= HandleRunStarted;
            if (_onRunEnded != null)
                _onRunEnded.OnEventRaised -= HandleRunEnded;

            _runIsActive = false;
            StopSlowMotion();
            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (_ballController != null)
                _ballController.ActiveShieldConsumed -= RefreshButtons;
        }

        private void HandleRunStarted()
        {
            _runIsActive = true;
            RefreshButtons();
        }

        private void HandleRunEnded()
        {
            _runIsActive = false;
            StopSlowMotion();
            RefreshButtons();
        }

        /// <summary>Creates one button for every powerup in the catalog.</summary>
        private void CreateButtonsFromCatalog()
        {
            if (_saveService == null)
            {
                Debug.LogError("PlayerPrefsSaveService is unavailable; powerup controls cannot be created.", this);
                return;
            }

            if (!ShopData.TryLoadItemsFromResources(out var items))
                return;

            foreach (var item in items)
            {
                if (item == null || item.type != ShopItemType.Powerup)
                    continue;

                var powerupButton = CreateButton(item);
                _buttons.Add(powerupButton);
                powerupButton.Button.onClick.AddListener(() => UsePowerup(powerupButton));
            }

            LayoutButtons();
        }

        private PowerupButton CreateButton(ShopItemDataModel item)
        {
            var buttonObject = new GameObject($"Powerup_{item.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(transform, false);

            var image = buttonObject.GetComponent<Image>();
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            image.color = item.powerupEffect == ShopPowerupEffect.ShieldNextHit
                ? new Color(0.16f, 0.42f, 0.56f, 0.96f)
                : new Color(0.52f, 0.31f, 0.16f, 0.96f);
            image.raycastTarget = true;

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 4f);
            labelRect.offsetMax = new Vector2(-6f, -4f);

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 27f;
            label.enableAutoSizing = true;
            label.fontSizeMin = 16f;
            label.fontSizeMax = 27f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            return new PowerupButton(item, button, label);
        }

        private void LayoutButtons()
        {
            var parentRect = transform as RectTransform;
            var availableWidth = parentRect != null && parentRect.rect.width > 0f
                ? parentRect.rect.width - 32f
                : Screen.width - 32f;
            const float spacing = 12f;
            const float preferredWidth = 220f;
            const float minWidth = 112f;

            var count = _buttons.Count;
            if (count == 0)
                return;

            var width = Mathf.Clamp((availableWidth - spacing * (count - 1)) / count, minWidth, preferredWidth);
            var totalWidth = width * count + spacing * (count - 1);
            for (var i = 0; i < count; i++)
            {
                var rect = _buttons[i].Button.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(width, 92f);
                rect.anchoredPosition = new Vector2(-totalWidth * 0.5f + width * (i + 0.5f) + spacing * i, 28f);
            }
        }

        /// <summary>Activates the selected effect and consumes one saved use.</summary>
        private void UsePowerup(PowerupButton powerupButton)
        {
            var item = powerupButton.Item;
            if (!CanUse(item))
                return;

            switch (item.powerupEffect)
            {
                case ShopPowerupEffect.ShieldNextHit:
                    if (_ballController == null || !_ballController.TryActivateShield())
                        return;
                    Consume(item.id);
                    break;

                case ShopPowerupEffect.SlowMotion:
                    if (_slowMotionCoroutine != null)
                        return;
                    Consume(item.id);
                    _slowMotionCoroutine = StartCoroutine(SlowMotionRoutine());
                    break;

                default:
                    Debug.LogWarning($"Powerup {item.id} has no implemented effect ({item.powerupEffect}).", this);
                    return;
            }

            RefreshButtons();
        }

        /// <summary>Checks run state, saved quantity, and effect-specific availability.</summary>
        private bool CanUse(ShopItemDataModel item)
        {
            if (!_runIsActive || _saveService == null || item == null ||
                _saveService.LoadShopProgress().GetPowerupCount(item.id) <= 0)
                return false;

            switch (item.powerupEffect)
            {
                case ShopPowerupEffect.ShieldNextHit:
                    return _ballController != null && !_ballController.HasActiveShield;
                case ShopPowerupEffect.SlowMotion:
                    return _slowMotionCoroutine == null;
                default:
                    return false;
            }
        }

        /// <summary>Decrements and persists one use of the selected powerup.</summary>
        private void Consume(int powerupId)
        {
            var progress = _saveService.LoadShopProgress();
            var currentCount = progress.GetPowerupCount(powerupId);
            if (currentCount <= 0)
                return;

            progress.SetPowerupCount(powerupId, currentCount - 1);
            _saveService.SaveShopProgress(progress);
        }

        private IEnumerator SlowMotionRoutine()
        {
            Time.timeScale = Mathf.Clamp(_slowMotionScale, 0.1f, 1f);
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, _slowMotionDuration));
            _slowMotionCoroutine = null;
            Time.timeScale = 1f;
            RefreshButtons();
        }

        private void StopSlowMotion()
        {
            if (_slowMotionCoroutine != null)
            {
                StopCoroutine(_slowMotionCoroutine);
                _slowMotionCoroutine = null;
            }

            Time.timeScale = 1f;
        }

        private void RefreshButtons()
        {
            foreach (var powerupButton in _buttons)
            {
                var count = _saveService == null
                    ? 0
                    : _saveService.LoadShopProgress().GetPowerupCount(powerupButton.Item.id);
                powerupButton.Label.text = $"{powerupButton.Item.name}\n{count}";
                powerupButton.Button.interactable = CanUse(powerupButton.Item);
            }
        }

        /// <summary>Groups the catalog data and UI references for one powerup control.</summary>
        private sealed class PowerupButton
        {
            public readonly ShopItemDataModel Item;
            public readonly Button Button;
            public readonly TextMeshProUGUI Label;

            /// <summary>Creates a view record for a powerup button and its label.</summary>
            public PowerupButton(ShopItemDataModel item, Button button, TextMeshProUGUI label)
            {
                Item = item;
                Button = button;
                Label = label;
            }
        }
    }
}
