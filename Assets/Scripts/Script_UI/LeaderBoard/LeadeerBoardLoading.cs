using ScriptableObjects.GameEvents;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    /// <summary>Shows and hides the leaderboard loading indicator in response to events.</summary>
    public class LeadeerBoardLoading : MonoBehaviour
    {
        /// <summary>Raised when a leaderboard tab requests data.</summary>
        public NullEvent OnLeaderBoardTabClicked;
        /// <summary>Raised when leaderboard data is ready for display.</summary>
        public NullEvent OnLeaderBoardDataLoaded;
        [SerializeField] private Transform icon;
        [SerializeField]private CanvasGroup canvasGroup;

        private void Awake()
        {
            icon.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            OnLeaderBoardTabClicked.OnEventRaised += ShowLoading;
            OnLeaderBoardDataLoaded.OnEventRaised += HideLoading;
        }

        private void OnDisable()
        {
            OnLeaderBoardTabClicked.OnEventRaised -= ShowLoading;
            OnLeaderBoardDataLoaded.OnEventRaised -= HideLoading;
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
