using UnityEngine;
using VRWeb.Utils;

namespace VRWeb.UI
{
	public class PopupIconProvider : SingletonMonoBehaviour<PopupIconProvider>
    {
        [SerializeField]
        private Sprite m_CheckMark = null;

        [SerializeField]
        private Sprite m_Cancel = null;

        // Getters
        public Sprite Checkmark => m_CheckMark;

        // Getters
        public Sprite Cancel => m_Cancel;
    }
}