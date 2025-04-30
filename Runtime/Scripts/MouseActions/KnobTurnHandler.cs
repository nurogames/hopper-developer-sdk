#if !VRWEB_TOOLKIT_ONLY
using System;
using UnityEngine;
using VRWeb.Managers;
using VRWeb.Rig;

namespace VRWeb.MouseActions
{
	public class KnobTurnHandler : HopperManagerMonoBehaviour<KnobTurnHandler>
    {
        [SerializeField] private LayerMask m_KnobLayerMask;

        private const float MOUSE_TURN_RANGE = 100f;

        private KnobData m_TurnedKnob = null;
        private KnobData m_HoveredKnob = null;

        private Vector2 m_InitialTurnMousePosition = Vector2.zero;
        private float m_InitialKnobValue = 0f;

        private bool m_CanMoveToggleButtonValue;

        // Getter
        public KnobData HoveredKnob => m_HoveredKnob;

        private void Awake()
        {
            RegisterManager();
        }

        private void Update()
        {
            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();

            if (vms == null || vms.IsInVrMode)
                return;

            if (m_TurnedKnob == null)
                m_HoveredKnob = GetClosestRaycastHitKnob();
            else
            {
                Vector2 mousePos = HopperRoot.Get < MouseHandler >().MousePosition.ReadValue<Vector2>();

                TurnKnob(mousePos);
            }
        }

        // Public functions
        public void OnMouseButtonDown()
        {
            if (HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            if (m_HoveredKnob == null)
                return;

            m_TurnedKnob = m_HoveredKnob;

            HopperRoot.Get < MouseHandler >().onLeftMouseButtonUp.AddListener(OnMouseButtonUp);

            m_InitialTurnMousePosition = HopperRoot.Get<MouseHandler>().MousePosition.ReadValue<Vector2>();

            m_InitialKnobValue = m_TurnedKnob.Value;

            m_CanMoveToggleButtonValue = HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn;

            HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn = true;
        }

        // Private functions
        private KnobData GetClosestRaycastHitKnob()
        {
            if ( HopperRoot.Get < MouseHandler >() == null)
                return null;

            RaycastHit[] raycastHits = null;

            try
            {
                raycastHits = HopperRoot.Get < MouseHandler >().TestHit( m_KnobLayerMask );
            }
            catch ( Exception e )
            {
                Debug.LogException( e );
            }

            if (raycastHits.Length == 0)
                return null;

            // Sort hits by distance
            System.Array.Sort(raycastHits, (a, b) => a.distance.CompareTo(b.distance));

            // Find first KnobData 
            foreach (RaycastHit hit in raycastHits)
            {
                GameObject hitObject = hit.transform.gameObject;

                if (hitObject != null && hitObject.GetComponentInParent<KnobData>() != null)
                {
                    return hitObject.GetComponentInParent<KnobData>();
                }
            }

            return null;
        }

        private void TurnKnob(Vector2 mousePos)
        {
            float minMouseOffset = -m_InitialKnobValue * MOUSE_TURN_RANGE;
            float maxMouseOffset = (1f - m_InitialKnobValue) * MOUSE_TURN_RANGE;

            float mouseOffset = mousePos.x - m_InitialTurnMousePosition.x;

            float newValue = m_InitialKnobValue + Mathf.Clamp(mouseOffset, minMouseOffset, maxMouseOffset) / MOUSE_TURN_RANGE;

            m_TurnedKnob.ChangeValue(Mathf.Clamp01(newValue));
        }

        private void OnMouseButtonUp()
        {
            m_TurnedKnob = null;

            HopperRoot.Get<ViewModeSwitcher>().CanMoveToggleButton.isOn = m_CanMoveToggleButtonValue;
        }
    }
}
#endif
