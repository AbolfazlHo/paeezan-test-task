using UnityEngine;
using DG.Tweening;
using Enums;
using ScriptableObjects.UIConfigs;
using ScriptableObjects.GameEvents;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>Moves tab content horizontally and maps drag gestures to tab selections.</summary>
    public class TabPanel : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TabType tab;
        [SerializeField] private IntEvent onTabSelected;

        [Header("Animation")]
        [SerializeField] private float animDuration = 0.4f;
        
        [Header("Swipe Settings")]
        [SerializeField] private float swipeThreshold = 50f;

        private PanelSequence panelSequence;
        private float offscreenX;

        /// <summary>Changes tabs when a completed drag exceeds the configured swipe threshold.</summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            float deltaX = eventData.pressPosition.x - eventData.position.x;

            if (Mathf.Abs(deltaX) < swipeThreshold)
                return;

            if (deltaX > 0)
                SwipeLeft();
            else
                SwipeRight();
        }

        private void SwipeLeft()
        {
            activeIndex = Mathf.Min(activeIndex + 1, panelSequence.sequence.Length - 1);
            onTabSelected.Raise(activeIndex);
        }

        private void SwipeRight()
        {
            activeIndex = Mathf.Max(activeIndex - 1, 0);
            onTabSelected.Raise(activeIndex);
        }
        
        private RectTransform rect;
        private static int activeIndex; 

        private Vector2 originalPos;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            panelSequence = PanelSequence.Main;
            activeIndex = (int)panelSequence.firstActiveTab;

            originalPos = rect.anchoredPosition;
            offscreenX = Mathf.Max(Screen.width,2000f);
        }

        void OnEnable()
        {
            onTabSelected.OnEventRaised += HandleTabSelected;
        }

        void OnDisable()
        {
            onTabSelected.OnEventRaised -= HandleTabSelected;
        }

        void Start()
        {
            onTabSelected.Raise(activeIndex);
        }

        private void HandleTabSelected(int selectedIndex)
        {
            activeIndex = selectedIndex;
            UpdatePosition(selectedIndex);
        }

        /// <summary>Moves this tab's panel to the selected or off-screen position.</summary>
        private void UpdatePosition(int centerIndex, bool instant = false)
        {
            int myIndex = System.Array.IndexOf(panelSequence.sequence, tab);
            Vector2 targetPos;

            if (myIndex < centerIndex)
                targetPos = originalPos + new Vector2(-offscreenX, 0);
            else if (myIndex > centerIndex)
                targetPos = originalPos + new Vector2(offscreenX, 0);
            else
                targetPos = originalPos;

            rect.DOKill();
            if (instant)
                rect.anchoredPosition = targetPos;
            else
                rect.DOAnchorPos(targetPos, animDuration).SetEase(Ease.OutCubic);
        }

        /// <summary>Receives drag callbacks; movement is applied when the drag ends.</summary>
        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}
