using UnityEngine;
using VRWeb.UI.Elements;

namespace VRWeb.UI
{
	[System.Serializable]
    public class PopupStyle
    {
        // Serialized variables
        [SerializeField] ButtonStyle m_ButtonStyle = null;
        [SerializeField] PanelStyle m_PanelStyle = null;

        // Getters
        public ButtonStyle ButtonStyle => m_ButtonStyle;
        public PanelStyle PanelStyle => m_PanelStyle;
    }

}