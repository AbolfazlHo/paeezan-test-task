using ScriptableObjects.GameEvents;
using UnityEngine;
using DG.Tweening;

namespace UI.Shop
{
    //ToDo: Create a base class for loading. ShopLoading and LeaderboardLaoding should inherit it.
    /// <summary>Shows and hides the shop loading indicator in response to shop events.</summary>
    public class ShopLoading : MonoBehaviour
    {
        /// <summary>Raised when the shop starts loading its content.</summary>
        public NullEvent OnShopDataLoading;
        /// <summary>Raised when the shop finishes loading its content.</summary>
        public NullEvent OnShopDataLoaded;
        [SerializeField] private Transform icon;
        [SerializeField]private CanvasGroup canvasGroup;

        private void Awake()
        {
            icon.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            OnShopDataLoading.OnEventRaised += ShowLoading;
            OnShopDataLoaded.OnEventRaised += HideLoading;
        }

        private void OnDisable()
        {
            OnShopDataLoading.OnEventRaised -= ShowLoading;
            OnShopDataLoaded.OnEventRaised -= HideLoading;
        }

        /// <summary>Displays and animates the loading icon.</summary>
        public void ShowLoading()
        {
            icon.gameObject.SetActive(true);
            icon.DOKill(true);


            canvasGroup.DOFade(1f, 0.05f).SetEase(Ease.OutCubic);
            icon.localScale = Vector3.one * 0.8f;
            icon.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

            
            icon.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);

        }

        /// <summary>Fades out the loading icon and disables it when the fade completes.</summary>
        public void HideLoading()
        {
            icon.DOKill();
            
            canvasGroup.DOFade(0f, 0.25f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    icon.gameObject.SetActive(false);
                });
        }
    }
}
