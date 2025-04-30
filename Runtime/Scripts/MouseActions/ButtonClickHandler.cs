#if !VRWEB_TOOLKIT_ONLY
using UnityEngine;
using VRWeb.Managers;
using VRWeb.Rig;
using WorldBuilder.Core.Components;

namespace VRWeb.MouseActions
{
	public class ButtonClickHandler : HopperManagerMonoBehaviour<ButtonClickHandler>
    {
        [SerializeField] private LayerMask m_ButtonLayerMask;

        private ButtonData m_PushedButton = null;
        private ButtonData m_HoveredButton = null;

        public ButtonData PushedButton => m_PushedButton;
        public ButtonData HoveredButton => m_HoveredButton;

        private void Awake()
        {
            RegisterManager();
        }

        private void Update()
        {
            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();

            if (vms == null || vms.IsInVrMode)
                return;

            m_HoveredButton = GetClosestRaycastHitButton();
        }

        // Public functions
        public void OnMouseButtonDown()
        {
            if (HopperRoot.Get<ViewModeSwitcher>() == null)
                return;

            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_HoveredButton == null)
                return;

            PushButton(m_HoveredButton);
        }

        // Private functions
        private ButtonData GetClosestRaycastHitButton()
        {
            if (HopperRoot.Get<MouseHandler>() == null)
                return null;

            RaycastHit[] raycastHits = HopperRoot.Get<MouseHandler>().TestHit(m_ButtonLayerMask);

            if (raycastHits.Length == 0)
                return null;

            // Sort hits by distance
            System.Array.Sort(raycastHits, (a, b) => a.distance.CompareTo(b.distance));

            // Find first ButtonData 
            foreach (RaycastHit hit in raycastHits)
            {
                GameObject hitObject = hit.transform.gameObject;

                if (hitObject != null && hitObject.GetComponentInParent<ButtonData>() != null)
                {
                    return hitObject.GetComponentInParent<ButtonData>();
                }
            }

            return null;
        }

        private void PushButton(ButtonData buttonData)
        {
            m_PushedButton = buttonData;

            m_PushedButton.InvokeTestPush(true);

            HopperRoot.Get<MouseHandler>().onLeftMouseButtonUp.AddListener(InvokeTestPushMouseUp);
        }

        private void InvokeTestPushMouseUp()
        {
            m_PushedButton.InvokeTestPush(false);

            HopperRoot.Get<MouseHandler>().onLeftMouseButtonUp.RemoveListener(InvokeTestPushMouseUp);

            m_PushedButton = null;
        }
    }
}
#endif
