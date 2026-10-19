using UnityEngine;
using System;
using NaughtyAttributes;

namespace ScriptableObjects.GameEvents
{
    /// <summary>ScriptableObject event channel that broadcasts one integer value.</summary>
[CreateAssetMenu(menuName = "GameEvents/IntEvent")]
    public class IntEvent : ScriptableObject
    {
        /// <summary>Subscribers invoked when an integer value is raised.</summary>
        public Action<int> OnEventRaised;
        /// <summary>Broadcasts the supplied value to current subscribers.</summary>
        public void Raise(int value)
        {
            OnEventRaised?.Invoke(value);
        }


        [Header("For Test Event")]
        /// <summary>Value used by the Inspector test button.</summary>
        public int test = 0;
        /// <summary>Broadcasts the configured test value from the Inspector.</summary>
        [Button]
        public void RaiseTest()
        {
            OnEventRaised?.Invoke(test);
        }
    }
}
