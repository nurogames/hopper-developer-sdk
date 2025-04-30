using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VRWeb.Managers;
using VRWeb.Rig;
using WorldBuilder.Core;

namespace VRWeb.MouseActions
{
	public class MouseHandler : HopperManagerMonoBehaviour<MouseHandler>
    {
        // Serialized variables
        [SerializeField] private InputAction m_LeftMouseButton;
        [SerializeField] private InputAction m_RightMouseButton;
        [SerializeField] private InputAction m_MousePosition;
        [SerializeField] private InputAction m_ControlKey;
        [SerializeField] private InputAction m_ScrollUp;
        [SerializeField] private InputAction m_ScrollDown;
        [SerializeField] private InputAction m_RotateYKey;
        [SerializeField] private InputAction m_RotateZKey;

        [SerializeField] private UnityEvent m_OnLeftMouseButtonDown;
        [SerializeField] private UnityEvent m_OnLeftMouseButtonUp;
        [SerializeField] private UnityEvent m_OnRightMouseButtonDown;
        [SerializeField] private UnityEvent m_OnRightMouseButtonUp;
        [SerializeField] private UnityEvent m_OnControlKeyDown;
        [SerializeField] private UnityEvent m_OnControlKeyUp;
        [SerializeField] private UnityEvent m_OnScrollUp;
        [SerializeField] private UnityEvent m_OnScrollDown;
        [SerializeField] private UnityEvent m_OnRotateYKeyDown;
        [SerializeField] private UnityEvent m_OnRotateYKeyUp;
        [SerializeField] private UnityEvent m_OnRotateZKeyDown;
        [SerializeField] private UnityEvent m_OnRotateZKeyUp;

        // Private variables
        private RaycastHit[] m_RaycastHits;
        private Vector3 m_RaycastDirection = Vector3.zero;
        private bool m_IsLeftButtonPressed = false;
        private bool m_IsRightButtonPressed = false;
        private bool m_IsControlKeyPressed = false;
        private bool m_IsRotateYKeyPressed = false;
        private bool m_IsRotateZKeyPressed = false;
        private Camera m_Camera = null;
        private bool m_IsMouseOutsideGameWindow = false;

        // Events
        public UnityEvent onLeftMouseButtonUp => m_OnLeftMouseButtonUp;

        public UnityEvent onRightMouseButtonUp => m_OnRightMouseButtonUp;

        public UnityEvent onLeftMouseButtonDown => m_OnLeftMouseButtonDown;

        public UnityEvent onRightMouseButtonDown => m_OnRightMouseButtonDown;

        // Getter
        public bool IsLeftButtonPressed => m_IsLeftButtonPressed;
        public bool IsRightButtonPressed => m_IsRightButtonPressed;

        public InputAction MousePosition => m_MousePosition;

        public Camera Camera => m_Camera;

        public Vector3 ScreenToPointDirection => m_RaycastDirection;

        public bool IsMouseOutsideGameWindow => m_IsMouseOutsideGameWindow;
        public bool BlockMouseActions { get; set; }

        private void Awake()
        {
            RegisterManager();
            BlockMouseActions = false;
        }

        // OnEnable function
        private void OnEnable()
        {
            m_LeftMouseButton.Enable();
            m_LeftMouseButton.started += OnMouseDown;
            m_LeftMouseButton.canceled += OnMouseUp;

            m_RightMouseButton.Enable();
            m_RightMouseButton.started += OnRightMouseButtonDown;
            m_RightMouseButton.canceled += OnRightMouseButtonUp;

            m_MousePosition.Enable();

            m_ControlKey.Enable();
            m_ControlKey.started += OnControlKeyDown;
            m_ControlKey.canceled += OnControlKeyUp;

            m_ScrollUp.Enable();
            m_ScrollUp.started += OnScrollUp;

            m_ScrollDown.Enable();
            m_ScrollDown.started += OnScrollDown;

            m_RotateYKey.Enable();
            m_RotateYKey.started += OnRotateYKeyDown;
            m_RotateYKey.canceled += OnRotateYKeyUp;

            m_RotateZKey.Enable();
            m_RotateZKey.started += OnRotateZKeyDown;
            m_RotateZKey.canceled += OnRotateZKeyUp;
        }

        // OnDisable function
        private void OnDisable()
        {
            m_LeftMouseButton.Disable();
            m_LeftMouseButton.started -= OnMouseDown;
            m_LeftMouseButton.canceled -= OnMouseUp;

            m_RightMouseButton.Disable();
            m_RightMouseButton.started -= OnRightMouseButtonDown;
            m_RightMouseButton.canceled -= OnRightMouseButtonUp;

            m_MousePosition.Disable();

            m_ControlKey.Disable();
            m_ControlKey.started -= OnControlKeyDown;
            m_ControlKey.canceled -= OnControlKeyUp;

            m_ScrollUp.Disable();
            m_ScrollUp.started -= OnScrollUp;

            m_ScrollDown.Disable();
            m_ScrollDown.started -= OnScrollDown;

            m_RotateYKey.Disable();
            m_RotateYKey.started -= OnRotateYKeyDown;
            m_RotateYKey.canceled -= OnRotateYKeyUp;

            m_RotateZKey.Disable();
            m_RotateZKey.started -= OnRotateZKeyDown;
            m_RotateZKey.canceled -= OnRotateZKeyUp;
        }

        // Update function
        private void Update()
        {
            BlockOnMouseOverUI();

            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();

            Vector2 mousePosition = m_MousePosition.ReadValue<Vector2>();
            m_IsMouseOutsideGameWindow = TestIsMouseOutsideGameWindow(mousePosition);

            if ( vms == null || vms.IsInVrMode || m_IsMouseOutsideGameWindow )
            {
                m_RaycastHits = null;
                m_Camera = null;
                return;
            }

            m_Camera = HopperRoot.Get<ViewModeSwitcher>().CamTransform?.GetComponent<Camera>();

            if (m_Camera == null)
            {
                m_RaycastHits = null;
                return;
            }

            Ray ray = m_Camera.ScreenPointToRay(mousePosition);

            m_RaycastDirection = ray.direction;
            m_RaycastHits = Physics.RaycastAll(ray, Mathf.Infinity);
        }

        // Public functions
        public RaycastHit[] TestHit(LayerMask mask)
        {
            List<RaycastHit> foundHits = new();
            if ( BlockMouseActions || 
                 HopperRoot.Get<ViewModeSwitcher>().IsInVrMode ||
                 m_RaycastHits == null || 
                 m_RaycastHits.Length == 0 )
                return foundHits.ToArray();

            foreach (RaycastHit raycastHit in m_RaycastHits)
            {
                if (raycastHit.transform == null)
                    continue;

                try
                {
                    LayerMask m = ( 1 << raycastHit.transform.gameObject.layer);

                    if ( ( m & mask ) != 0 )
                    {
                        foundHits.Add( raycastHit );
                    }
                }
                catch ( Exception e )
                {
                    Debug.LogException( e );
                }
            }

            return foundHits.ToArray();
        }

        List <GraphicRaycaster> m_RayCasterList = new List<GraphicRaycaster>();

        public void RegisterGraphicRaycaster(GraphicRaycaster graphicRaycaster)
        {
            m_RayCasterList.Add( graphicRaycaster );
        }

        public void UnregisterGraphicRaycaster(GraphicRaycaster graphicRaycaster)
        {
            m_RayCasterList.Remove(graphicRaycaster);
        }

        private void BlockOnMouseOverUI()
        {
            if ( HopperRoot.Get < ViewModeSwitcher >() == null )
                return;

            if ( HopperRoot.Get < ViewModeSwitcher >().IsInVrMode )
            {
                return;
            }

            Vector2 mousePosition = m_MousePosition.ReadValue<Vector2>();

            BlockMouseActions = false;
            
            foreach (GraphicRaycaster rayCaster in m_RayCasterList)
            {
                List<RaycastResult> rayResults = new();
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                pointerEventData.position = mousePosition;
                rayCaster.Raycast(pointerEventData, rayResults);
                if (rayResults.Count > 0)
                {
                    BlockMouseActions = true;
                    return;
                }
            }
        }

        private void OnMouseDown(InputAction.CallbackContext cc)
        {
            m_IsLeftButtonPressed = cc.ReadValueAsButton();
            if (!m_IsLeftButtonPressed)
                return;
            m_OnLeftMouseButtonDown?.Invoke();
        }

        private void OnMouseUp(InputAction.CallbackContext cc)
        {
            m_IsLeftButtonPressed = cc.ReadValueAsButton();
            if (m_IsLeftButtonPressed)
                return;

            m_OnLeftMouseButtonUp?.Invoke();
        }

        private void OnRightMouseButtonDown(InputAction.CallbackContext cc)
        {
            m_IsRightButtonPressed = cc.ReadValueAsButton();
            if (!m_IsRightButtonPressed)
                return;

            m_OnRightMouseButtonDown?.Invoke();
        }

        private void OnRightMouseButtonUp(InputAction.CallbackContext cc)
        {
            m_IsRightButtonPressed = cc.ReadValueAsButton();
            if (m_IsRightButtonPressed)
                return;

            m_OnRightMouseButtonUp?.Invoke();
        }

        private void OnControlKeyDown(InputAction.CallbackContext cc)
        {
            m_IsControlKeyPressed = cc.ReadValueAsButton();

            if (!m_IsControlKeyPressed)
                return;

            m_OnControlKeyDown?.Invoke();
        }

        private void OnControlKeyUp(InputAction.CallbackContext cc)
        {
            m_IsControlKeyPressed = cc.ReadValueAsButton();

            if (m_IsControlKeyPressed)
                return;

            m_OnControlKeyUp?.Invoke();
        }

        private void OnScrollUp(InputAction.CallbackContext cc)
        {
            m_OnScrollUp?.Invoke();
        }

        private void OnScrollDown(InputAction.CallbackContext cc)
        {
            m_OnScrollDown?.Invoke();
        }

        private void OnRotateYKeyDown(InputAction.CallbackContext cc)
        {
            m_IsRotateYKeyPressed = cc.ReadValueAsButton();

            if (!m_IsRotateYKeyPressed)
                return;

            m_OnRotateYKeyDown?.Invoke();
        }

        private void OnRotateYKeyUp(InputAction.CallbackContext cc)
        {
            m_IsRotateYKeyPressed = cc.ReadValueAsButton();

            if (m_IsRotateYKeyPressed)
                return;

            m_OnRotateYKeyUp?.Invoke();
        }

        private void OnRotateZKeyDown(InputAction.CallbackContext cc)
        {
            m_IsRotateZKeyPressed = cc.ReadValueAsButton();

            if (!m_IsRotateZKeyPressed)
                return;

            m_OnRotateZKeyDown?.Invoke();
        }

        private void OnRotateZKeyUp(InputAction.CallbackContext cc)
        {
            m_IsRotateZKeyPressed = cc.ReadValueAsButton();

            if (m_IsRotateZKeyPressed)
                return;

            m_OnRotateZKeyUp?.Invoke();
        }

        private bool TestIsMouseOutsideGameWindow( Vector2 mousePosition )
        {
            Vector2 screenSize = new Vector2( Screen.width, Screen.height );
            return ( mousePosition.x <= 0 || mousePosition.y <= 0
                     || mousePosition.x >= screenSize.x || mousePosition.y >= screenSize.y );
        }
    }
}