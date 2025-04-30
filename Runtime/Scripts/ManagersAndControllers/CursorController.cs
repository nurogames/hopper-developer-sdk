#if !VRWEB_TOOLKIT_ONLY
using UnityEngine;
using VRWeb.MouseActions;
using VRWeb.Rig;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace VRWeb.Managers
{
	public class CursorController : HopperManagerMonoBehaviour<CursorController>, ICursorController
    {
        // Serialized variables
        [SerializeField] private Texture2D m_GrabCursor = null;
        [SerializeField] private Texture2D m_ApplyCursor = null;
        [SerializeField] private Texture2D m_PressCursor = null;
        [SerializeField] private Texture2D m_DragCursor = null;
        [SerializeField] private Texture2D m_TeleportCursor = null;

        [SerializeField] private MouseGrabber m_MouseGrabber = null;
        [SerializeField] private ButtonClickHandler m_ButtonClickHandler = null;
        [SerializeField] private SliderDragHandler m_SliderDragHandler = null;
        [SerializeField] private KnobTurnHandler m_KnobTurnHandler = null;
        [SerializeField] private MouseTeleporter m_MouseTeleporter = null;

        private ICursorController.Modes m_CurrentCursorMode = ICursorController.Modes.Default;

        public ICursorController.Modes CurrentCursorMode => m_CurrentCursorMode;

        private void Awake()
        {
            RegisterManager();
        }

        private void Update()
        {
            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();

            if (vms == null || vms.IsInVrMode || HopperRoot.Get<MouseHandler>().IsMouseOutsideGameWindow)
            {
                SetCursor( ICursorController.Modes.Default );
                return;
            }

            if (m_ButtonClickHandler != null && m_ButtonClickHandler.PushedButton != null)
                m_CurrentCursorMode = ICursorController.Modes.Press;
            else if (m_MouseGrabber != null && m_MouseGrabber.GrabbedInteractable != null)
                m_CurrentCursorMode = ICursorController.Modes.Grab;
            else if (m_ButtonClickHandler != null && m_ButtonClickHandler.HoveredButton != null)
                m_CurrentCursorMode = ICursorController.Modes.Press;
            else if (m_MouseGrabber != null && m_MouseGrabber.HoveredInteractable != null)
                m_CurrentCursorMode = ICursorController.Modes.Grab;
            else if (m_SliderDragHandler != null && m_SliderDragHandler.HoveredSlider != null)
                m_CurrentCursorMode = ICursorController.Modes.Drag;
            else if ( m_KnobTurnHandler != null && m_KnobTurnHandler.HoveredKnob != null )
                m_CurrentCursorMode = ICursorController.Modes.Drag;
            else if ( m_MouseTeleporter != null && m_MouseTeleporter.HoveredFloor != null )
                m_CurrentCursorMode = ICursorController.Modes.Teleport;
            else
                m_CurrentCursorMode = ICursorController.Modes.Default;

            SetCursor( m_CurrentCursorMode );
        }

        // Public functions
        public void SetCursor(ICursorController.Modes mode)
        {
            Texture2D cursorTexture = null;

            switch( mode)
            {
                case ICursorController.Modes.Default:
                    cursorTexture = null;
                    break;
                case ICursorController.Modes.Grab:
                    cursorTexture = m_GrabCursor;
                    break;
                case ICursorController.Modes.Apply:
                    cursorTexture = m_ApplyCursor;
                    break;
                case ICursorController.Modes.Press:
                    cursorTexture = m_PressCursor;
                    break;
                case ICursorController.Modes.Drag:
                    cursorTexture = m_DragCursor;
                    break;
                case ICursorController.Modes.Teleport:
                    cursorTexture = m_TeleportCursor;
                    break;
            }

            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
    }
}
#endif
