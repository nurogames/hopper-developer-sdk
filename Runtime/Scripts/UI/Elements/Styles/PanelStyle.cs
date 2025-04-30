using UnityEngine;

namespace VRWeb.UI.Elements
{
	[System.Serializable]
    public class PanelStyle
    {
        // Serialized variables
        [SerializeField]
        private GameObject m_VrPrefab = null;
        [SerializeField]
        private GameObject m_UiPrefab = null;

        [SerializeField]
        private float m_OpenAnimationDuration = 0.3f;
        [SerializeField]
        private AnimationCurve m_OpenScaleCurve = new AnimationCurve();

        // Getter
        public GameObject VrPrefab => m_VrPrefab;
        public GameObject UiPrefab => m_UiPrefab;
        public float OpenAnimationDuration => m_OpenAnimationDuration;
        public AnimationCurve OpenScaleCurve => m_OpenScaleCurve;
    }

}