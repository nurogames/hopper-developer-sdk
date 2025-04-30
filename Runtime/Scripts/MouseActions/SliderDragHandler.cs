#if !VRWEB_TOOLKIT_ONLY
using System.Collections;
using UnityEngine;
using VRWeb.Managers;
using VRWeb.Rig;
using WorldBuilder.Core.Components;

namespace VRWeb.MouseActions
{
	public class SliderDragHandler : HopperManagerMonoBehaviour<SliderDragHandler>
    {
        [SerializeField] private LayerMask m_SliderLayerMask;

        private const float MOUSE_TO_SLIDER_RATIO = 100;

        private SliderData m_DraggedSlider = null;
        private SliderData m_HoveredSlider = null;

        private Vector2 m_MouseButtonDownPosition;
        private float m_OldSliderValue;

        private bool m_CanMoveToggleButtonValue;

        // Getter
        public SliderData HoveredSlider => m_HoveredSlider;

        private void Awake()
        {
            RegisterManager();
        }

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => HopperRoot.Get<MouseHandler>() == null);

            HopperRoot.Get < MouseHandler >().onLeftMouseButtonUp.AddListener(OnMouseButtonUp);
        }

        private void OnDisable()
        {
            HopperRoot.Get < MouseHandler >().onLeftMouseButtonUp.RemoveListener(OnMouseButtonUp);
        }

        private void Update()
        {
            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();

            if (vms == null || vms.IsInVrMode)
                return;

            if (m_DraggedSlider == null)
                m_HoveredSlider = GetClosestRaycastHitSlider();
            else
                DragSlider();
        }

        // Public functions
        public void OnMouseButtonDown()
        {
            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_HoveredSlider == null)
                return;

            m_DraggedSlider = m_HoveredSlider;
            m_MouseButtonDownPosition = HopperRoot.Get < MouseHandler >().MousePosition.ReadValue<Vector2>();
            m_OldSliderValue = m_DraggedSlider.Value;

            m_CanMoveToggleButtonValue = HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn;

            HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn = true;
        }

        // Private functions
        private SliderData GetClosestRaycastHitSlider()
        {
            RaycastHit[] raycastHits = HopperRoot.Get < MouseHandler >().TestHit(m_SliderLayerMask);

            if (raycastHits.Length == 0)
                return null;

            // Sort hits by distance
            System.Array.Sort(raycastHits, (a, b) => a.distance.CompareTo(b.distance));

            // Find first SliderData 
            foreach (RaycastHit hit in raycastHits)
            {
                GameObject hitObject = hit.transform.gameObject;

                if (hitObject != null && hitObject.GetComponentInParent<SliderData>() != null)
                {
                    return hitObject.GetComponentInParent<SliderData>();
                }
            }

            return null;
        }

        private void DragSlider()
        {
            Vector2 mouseOffset = HopperRoot.Get < MouseHandler >().MousePosition.ReadValue<Vector2>() - m_MouseButtonDownPosition;
            float newSliderValue = m_OldSliderValue + mouseOffset.x / MOUSE_TO_SLIDER_RATIO;

            m_DraggedSlider.ChangeValue(newSliderValue);
        }

        private void OnMouseButtonUp()
        {
            if (m_DraggedSlider != null)
            {
                m_DraggedSlider = null;

                HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn = m_CanMoveToggleButtonValue;
            }
        }
    }
}
#endif