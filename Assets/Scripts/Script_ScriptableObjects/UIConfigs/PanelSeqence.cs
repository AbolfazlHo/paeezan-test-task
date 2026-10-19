using Enums;

namespace ScriptableObjects.UIConfigs
{
    using UnityEngine;

    /// <summary>Stores the ordered tabs and initial selection used by the tab panel.</summary>
    [CreateAssetMenu(menuName = "UI/PanelSequence")]
    public class PanelSequence : ScriptableObject
    {
        /// <summary>Tab order used for horizontal navigation.</summary>
        public TabType[] sequence;

        /// <summary>Loads the shared panel-sequence asset from Resources.</summary>
        public static PanelSequence Main
        {
            get
            {
                if (_main == null)
                {
                    _main = Resources.Load<PanelSequence>("ScriptableObject_Panel_Sequence");
                }
                return _main;
            }
        }
        private static PanelSequence _main;

        /// <summary>Tab selected when the panel is initialized.</summary>
        public TabType firstActiveTab;
        
    }

}
