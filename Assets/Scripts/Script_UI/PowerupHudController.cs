using System.Collections;
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
    /// <summary>
    /// Builds the in-run powerup controls and consumes persisted inventory only
    /// while the run is active.
    /// </summary>
    public class PowerupHudController : MonoBehaviour
    {
        [SerializeField] private NullEvent _onRunStarted;
        [SerializeField] private NullEvent _onRunEnded;
        [SerializeField] private int _shieldPowerupId = 301;
        [SerializeField] private int _slowMotionPowerupId = 302;
        [SerializeField] private float _slowMotionScale = 0.5f;
        [SerializeField] private float _slowMotionDuration = 5f;

        private Button _shieldButton;
        private TextMeshProUGUI _shieldLabel;
        private Button _slowMotionButton;
        private TextMeshProUGUI _slowMotionLabel;
        private BallController _ballController;
        private PlayerPrefsSaveService _saveService;
        private Coroutine _slowMotionCoroutine;
        private bool _runIsActive;

        private void Awake()
        {
            _ballController = FindFirstObjectByType<BallController>();
            if (_ballController != null)
                _ballController.ActiveShieldConsumed += HandleShieldConsumed;
            _saveService = PlayerPrefsSaveService.Main;
            CreatePowerupButtons();
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

            StopSlowMotion();
        }

        private void OnDestroy()
        {
            if (_ballController != null)
                _ballController.ActiveShieldConsumed -= HandleShieldConsumed;
            if (_shieldButton != null)
                _shieldButton.onClick.RemoveListener(UseShield);
            if (_slowMotionButton != null)
                _slowMotionButton.onClick.RemoveListener(UseSlowMotion);
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

        private void HandleShieldConsumed()
        {
            RefreshButtons();
        }

        private void UseShield()
        {
            if (!CanUse(_shieldPowerupId, ShopPowerupEffect.ShieldNextHit) ||
                _ballController == null || !_ballController.TryActivateShield())
                return;

            Consume(_shieldPowerupId);
        }

        private void UseSlowMotion()
        {
            if (!CanUse(_slowMotionPowerupId, ShopPowerupEffect.SlowMotion) ||
                _slowMotionCoroutine != null)
                return;

            Consume(_slowMotionPowerupId);
            _slowMotionCoroutine = StartCoroutine(SlowMotionRoutine());
            RefreshButtons();
        }

        private bool CanUse(int powerupId, ShopPowerupEffect expectedEffect)
        {
            if (!_runIsActive || _saveService == null ||
                !ShopData.TryLoadItemFromResources(powerupId, out var item) ||
                item.type != ShopItemType.Powerup || item.powerupEffect != expectedEffect)
                return false;

            return _saveService.LoadShopProgress().GetPowerupCount(powerupId) > 0;
        }

        private void Consume(int powerupId)
        {
            var progress = _saveService.LoadShopProgress();
            var currentCount = progress.GetPowerupCount(powerupId);
            if (currentCount <= 0)
                return;

            progress.SetPowerupCount(powerupId, currentCount - 1);
            _saveService.SaveShopProgress(progress);
            RefreshButtons();
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

        private void CreatePowerupButtons()
        {
            _shieldButton = CreateButton("ShieldPowerupButton", -135f, out _shieldLabel);
            _slowMotionButton = CreateButton("SlowMotionPowerupButton", 135f, out _slowMotionLabel);
            _shieldButton.onClick.AddListener(UseShield);
            _slowMotionButton.onClick.AddListener(UseSlowMotion);
        }

        private Button CreateButton(string objectName, float xPosition, out TextMeshProUGUI label)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(transform, false);

            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(xPosition, 0f);
            buttonRect.sizeDelta = new Vector2(240f, 112f);

            var image = buttonObject.GetComponent<Image>();
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            image.color = objectName == "ShieldPowerupButton"
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
            labelRect.offsetMin = new Vector2(8f, 6f);
            labelRect.offsetMax = new Vector2(-8f, -6f);

            label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 27f;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = 27f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            return button;
        }

        private void RefreshButtons()
        {
            var shieldCount = GetCount(_shieldPowerupId);
            var slowMotionCount = GetCount(_slowMotionPowerupId);

            if (_shieldLabel != null)
                _shieldLabel.text = $"{GetName(_shieldPowerupId, "Shield")}\n{shieldCount}";
            if (_slowMotionLabel != null)
                _slowMotionLabel.text = $"{GetName(_slowMotionPowerupId, "Slow Time")}\n{slowMotionCount}";

            if (_shieldButton != null)
                _shieldButton.interactable = _runIsActive && shieldCount > 0 &&
                                             _ballController != null && !_ballController.HasActiveShield;
            if (_slowMotionButton != null)
                _slowMotionButton.interactable = _runIsActive && slowMotionCount > 0 &&
                                                 _slowMotionCoroutine == null;
        }

        private int GetCount(int powerupId)
        {
            return _saveService == null ? 0 : _saveService.LoadShopProgress().GetPowerupCount(powerupId);
        }

        private static string GetName(int powerupId, string fallback)
        {
            return ShopData.TryLoadItemFromResources(powerupId, out var item) ? item.name : fallback;
        }
    }
}
