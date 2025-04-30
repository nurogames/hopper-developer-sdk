using UnityEngine;

namespace VRWeb.UI.Elements
{
	[System.Serializable]
    public class ButtonStyle
    {
        [SerializeField]
        private GameObject m_VrTextButtonPrefab = null;

        [SerializeField]
        private GameObject m_UiTextButtonPrefab = null;

        [SerializeField]
        private GameObject m_VrIconButtonPrefab = null;

        [SerializeField]
        private GameObject m_UiIconButtonPrefab = null;

        // Getters
        public GameObject VrTextButtonPrefab => m_VrTextButtonPrefab;
        public GameObject UiTextButtonPrefab => m_UiTextButtonPrefab;

        public GameObject VrIconButtonPrefab => m_VrIconButtonPrefab;
        public GameObject UiIconButtonPrefab => m_UiIconButtonPrefab;
    }

}