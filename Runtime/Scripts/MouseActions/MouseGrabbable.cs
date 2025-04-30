#if !VRWEB_TOOLKIT_ONLY
using UnityEngine;

namespace VRWeb.MouseActions
{
	public class MouseGrabbable : MonoBehaviour, IMouseGrabbable
    {
        private MouseGrabber m_MouseGrabber = null;

        public void SetMouseGrabber(MouseGrabber mouseGrabber)
        {
            m_MouseGrabber = mouseGrabber;
        }

        public void Release()
        {
            m_MouseGrabber.ReleaseGrabbedInteractable();
        }
    }
}
#endif
