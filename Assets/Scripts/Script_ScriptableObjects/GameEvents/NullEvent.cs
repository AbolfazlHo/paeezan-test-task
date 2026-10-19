using UnityEngine;
using System;
using NaughtyAttributes;


namespace ScriptableObjects.GameEvents
{
    /// <summary>ScriptableObject event channel that broadcasts a signal without data.</summary>
[CreateAssetMenu(menuName = "GameEvents/NullEvent")]
    public class NullEvent : ScriptableObject
    {
        /// <summary>Subscribers invoked when the signal is raised.</summary>
        public Action OnEventRaised;
        /// <summary>Broadcasts the signal to current subscribers.</summary>
        public void Raise()
        {
            OnEventRaised?.Invoke();
        }
        /// <summary>Broadcasts the signal from the Inspector for manual checks.</summary>
        [Button]
        public void RaiseTest()
        {
            OnEventRaised?.Invoke();
        }
    }
}
