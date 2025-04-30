using UnityEngine;

namespace VRWeb.UI
{

	public class VRPopupDisplayer : MonoBehaviour
    {

        Coroutine m_ShowCoroutine = null;
        Coroutine m_HideCoroutine = null;

        public bool ShowCoroutineIsRunning => m_ShowCoroutine != null;
        public bool HideCoroutineIsRunning => m_HideCoroutine != null;


        public void Show(Popup popup, PopupStyle defaultStyle)
        {
            StopShowingCurrentPopup();
        }

        public void Hide()
        {

        }

        private void StopShowingCurrentPopup()
        {
            if (m_ShowCoroutine != null)
            {
                StopCoroutine(m_ShowCoroutine);
                m_ShowCoroutine = null;
            }

            if (m_HideCoroutine != null)
            {
                StopCoroutine(m_HideCoroutine);
                m_HideCoroutine = null;
            }
        }

    }

}