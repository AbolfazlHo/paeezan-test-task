using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>Adds a pressed-text offset and delayed reactivation helper to a Unity button.</summary>
    public class CustomizeButton : UnityEngine.UI.Button
    {
        [Header("Text Press Effect")]
        private RectTransform buttonText;
        private float pressOffsetY = -15f;
        private Vector3 originalTextPos;

        [Header("Auto Reactivate")]
        public float reactivateDelay = 3f;
        private bool waiting;

        protected override void Start()
        {
            base.Start();
            buttonText = transform.GetChild(0).GetComponent<RectTransform>();
            if (buttonText != null)
                originalTextPos = buttonText.localPosition;
        }
        
        /// <summary>Runs the base pointer-down behavior and presses the button label visually.</summary>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (buttonText != null)
                buttonText.localPosition = originalTextPos + new Vector3(0, pressOffsetY, 0);
        }

        /// <summary>Runs the base pointer-up behavior and restores the label position.</summary>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (buttonText != null)
                buttonText.localPosition = originalTextPos;
        }

        /// <summary>Re-enables the button after its configured delay when it is inactive.</summary>
        public  void ActivateInteractableAfterDelay()
        {
            if (!interactable && !waiting && gameObject.activeInHierarchy)
                StartCoroutine(ReEnableAfterDelay());
        }

        private System.Collections.IEnumerator ReEnableAfterDelay()
        {
            waiting = true;
            yield return new WaitForSeconds(reactivateDelay);
            interactable = true;
            waiting = false;
        }
    }
}
