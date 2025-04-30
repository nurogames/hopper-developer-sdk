using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Management;
using VRWeb.Avatar;
using VRWeb.Events;
using VRWeb.Managers;

namespace VRWeb.Rig
{
	public class ViewModeSwitcher : HopperManagerMonoBehaviour<ViewModeSwitcher>
    {
        [SerializeField]
        private GameObject m_XrOrigin;

        [SerializeField]
        private GameObject m_AvatarGO;

        [SerializeField]
        private ViewModeChangedEvent m_ViewModeChangedEvent;

        [SerializeField]
        private InputActionReference m_SwitchToVrAction;

        [SerializeField]
        private InputActionReference m_SwitchTo1stAction;

        [SerializeField]
        private InputActionReference m_SwitchTo3rdAction;

        [Tooltip("The virtual Camera which follows the player around")]
        [SerializeField]
        private GameObject m_PlayerFollowCamera;

        [Tooltip("The actual Camera which are the eyes of the player")]
        [SerializeField]
        private Camera m_AvatarCamera;

        [Tooltip("The Camera layers to be visible when in first person Mode")]
        [SerializeField]
        private LayerMask m_FirstPersonLayerMask;

        [Tooltip("The Camera layers to be visible when in third person Mode")]
        [SerializeField]
        private LayerMask m_ThirdPersonLayerMask;

        [SerializeField]
        private InputActionReference m_ToggleEnableMovement;

        [SerializeField]
        private StarterAssetsInputs m_StarterAssetsInputs;

        [SerializeField]
        private AvatarController m_AvatarController;

        [SerializeField]
        private Toggle m_CanMoveToggleButton;

        public bool IsInVrMode => m_CurrentMode == ViewModes.Vr;

        public bool IsXrInitialized => XRGeneralSettings.Instance.Manager.activeLoader != null;

        // Private variables
        //private bool m_WasUserPresent = false;

        private ViewModes m_CurrentMode = ViewModes.undefined;
        private ViewModes m_LastNonVRMode = ViewModes.ThirdPerson;

        private bool m_AutoSwitchingEnabled = true;

        private Coroutine m_LoaderStartCoroutine = null;
        private Coroutine m_XRRigCoroutine = null;

        private bool m_MovementIsEnabled = true;

        public ViewModes CurrentViewMode => m_CurrentMode;

        // Events
        public event Action<ViewModes> onModeChanged;

        // Getters
        public ViewModes CurrentMode => m_CurrentMode;

        public ViewModes LastNonVRMode => m_LastNonVRMode;

        public bool AutoSwitchingEnabled => m_AutoSwitchingEnabled;

        public bool MovementIsEnabled => !m_MovementIsEnabled;

        public Transform CamTransform => (IsInVrMode ? m_XrOrigin : m_AvatarCamera.gameObject).transform;

        public Transform PlayerTransform
        {
            get
            {
                if ( IsInVrMode)
                    return UserRig.Instance.HeadTransform;
                else
                    return m_AvatarGO.transform;
            }
        }

        public CapsuleCollider m_AvatarCapsuleCollider;

        public Toggle CanMoveToggleButton => m_CanMoveToggleButton;

        #region Unity Event Functions

        private void Awake()
        {
            RegisterManager();
            Application.targetFrameRate = 60;
        }

        private void OnEnable()
        {
            m_SwitchToVrAction.action.started += OnViewModeVR;
            m_SwitchTo1stAction.action.started += OnViewMode1stPerson;
            m_SwitchTo3rdAction.action.started += OnViewMode3rdPerson;
            m_ToggleEnableMovement.action.started += OnToggleEnableMovement;
            m_CanMoveToggleButton.SetIsOnWithoutNotify(MovementIsEnabled);
        }

        private void OnDisable()
        {
            m_SwitchToVrAction.action.started -= OnViewModeVR;
            m_SwitchTo1stAction.action.started -= OnViewMode1stPerson;
            m_SwitchTo3rdAction.action.started -= OnViewMode3rdPerson;
            m_ToggleEnableMovement.action.started -= OnToggleEnableMovement;
        }

        private void Start()
        {
            //Todo: M�ssen uns gedanken machen wie wir in Zukunft damit umgehen
            //switch (Application.platform)
            //{
            //    case RuntimePlatform.WindowsPlayer:
            //    case RuntimePlatform.WindowsEditor:
            //    case RuntimePlatform.OSXPlayer:
            //    case RuntimePlatform.OSXEditor:
            //        StartCoroutine(ActivateXRRigCoroutine());
            //        break;

            //    case RuntimePlatform.Android:
            //        m_CurrentMode = ViewModes.Vr;
            //        StartCoroutine(ActivateXRRigCoroutine());
            //        break;
            //}

            m_MovementIsEnabled = false;
            m_CanMoveToggleButton.SetIsOnWithoutNotify(m_MovementIsEnabled);

            ChangeMode(ViewModes.Vr);
        }

        #endregion

        #region Public

        public static bool isHMDPresent()
        {
            List<XRDisplaySubsystem> xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems<XRDisplaySubsystem>(xrDisplaySubsystems);

            foreach (XRDisplaySubsystem xrDisplay in xrDisplaySubsystems)
            {
                if (xrDisplay.running)
                {
                    return true;
                }
            }

            return false;
        }

        public void ActivateFirstPersonMode()
        {
            ActivateAvatarMode(ViewModes.FirstPerson);
        }

        public void ActivateThirdPersonMode()
        {
            ActivateAvatarMode(ViewModes.ThirdPerson);
        }

        public void ActivateAvatarMode(ViewModes mode)
        {
            m_LastNonVRMode = mode;
            ViewModes currentMode = m_CurrentMode;
            m_CurrentMode = mode;

            if (currentMode == ViewModes.Vr || currentMode == ViewModes.undefined)
            {
                m_ViewModeChangedEvent.OnViewModeChanged(ViewModes.Vr, mode);
                m_CanMoveToggleButton.gameObject.SetActive(true);

                HopperRoot.Get<RigPositioner>().SetPosDirFromRigToAvatar();
            }
            else
            {
                m_ViewModeChangedEvent.OnViewModeChanged(currentMode, mode);
            }

            StartCoroutine(SwitchCameras());

            if (UserRig.Instance.CurrentAvatar != null)
            {
                Collider collider = UserRig.Instance.CurrentAvatar.transform.parent.gameObject.
                                            GetComponent<Collider>();

                collider.enabled = true;
            }

            m_AvatarController.SetZoomAbs(mode == ViewModes.FirstPerson ? 0 : 4);

            StopXR();
        }

        public void ActivateXRMode()
        {
            if ( m_XRRigCoroutine != null )
                return;
            
            m_XRRigCoroutine = StartCoroutine(ActivateXRRigCoroutine());
        }

        public IEnumerator ActivateXRRigCoroutine()
        {
            if (m_CurrentMode == ViewModes.Vr)
            {
                m_XRRigCoroutine = null;

                yield break;
            }

            StartXR();

            yield return m_LoaderStartCoroutine;

            if (UserRig.Instance.CurrentAvatar != null)
            {
                HopperRoot.Get<RigPositioner>().SetPosDirFromAvatarToRig();
            }

            if (m_CurrentMode != ViewModes.Vr)
            {
                m_XRRigCoroutine = null;

                yield break;
            }

            yield return SwitchCameras();

            m_ViewModeChangedEvent.OnViewModeChanged(m_LastNonVRMode, ViewModes.Vr);
            m_CanMoveToggleButton.gameObject.SetActive(false);

            if (UserRig.Instance.CurrentAvatar != null)
            {
                Collider collider = UserRig.Instance.CurrentAvatar.transform.parent.gameObject.
                                                  GetComponent<Collider>();

                collider.enabled = false;
            }

            m_XRRigCoroutine = null;
        }

        public void ChangeMode(ViewModes mode)
        {
            switch (mode)
            {
                case ViewModes.Vr:
                    if ( m_XRRigCoroutine == null )
                        m_XRRigCoroutine = StartCoroutine( ActivateXRRigCoroutine() );

                    break;

                case ViewModes.FirstPerson:
                    ActivateFirstPersonMode();

                    break;

                case ViewModes.ThirdPerson:
                    ActivateThirdPersonMode();

                    break;
            }

            onModeChanged?.Invoke(m_CurrentMode);
        }

        public void DisableAutoSwitching()
        {
            m_AutoSwitchingEnabled = false;
        }

        public Vector3 GetUserForward()
        {
            Vector3 forward;

            if (m_CurrentMode == ViewModes.Vr)
            {
                forward = UserRig.Instance.HeadTransform.forward;
                forward.y = 0;
            }
            else
            {
                forward = UserRig.Instance.CurrentAvatar.transform.forward;
            }

            return forward;
        }

        public bool IsXRActive()
        {
            XRGeneralSettings settings = XRGeneralSettings.Instance;

            if (settings == null)
            {
                return false;
            }

            XRManagerSettings managerSettings = settings.Manager;

            if (managerSettings == null)
            {
                return false;
            }

            XRLoader loader = managerSettings.activeLoader;

            if (loader == null)
            {
                //m_LoaderStartCoroutine = StartCoroutine( StartXR() );
                return false;
            }

            return true;
        }

        public void OnViewMode1stPerson(InputAction.CallbackContext context)
        {
            ChangeMode(ViewModes.FirstPerson);
        }

        public void OnViewMode3rdPerson(InputAction.CallbackContext context)
        {
            ChangeMode(ViewModes.ThirdPerson);
        }

        public void OnViewModeVR(InputAction.CallbackContext context)
        {
            ChangeMode(ViewModes.Vr);
        }

        public void OnToggleEnableMovement(InputAction.CallbackContext context)
        {
            m_MovementIsEnabled = !m_MovementIsEnabled;
            m_CanMoveToggleButton.SetIsOnWithoutNotify(m_MovementIsEnabled);
        }

        public void OnEnableMovement(bool enabled)
        {
            m_MovementIsEnabled = enabled;
        }

        public IEnumerator StartXRCoroutine()
        {
            if (XRGeneralSettings.Instance == null)
            {
                m_LoaderStartCoroutine = null;
                yield break;
            }

            if (!IsXRDeviceConnected())
            {
                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            }

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogWarning("Initializing XR Failed. Changing to 3rd Person Mode.");
                ChangeMode(ViewModes.ThirdPerson);
            }
            else
            {
                Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                m_CurrentMode = ViewModes.Vr;
            }

            m_LoaderStartCoroutine = null;
        }

        #endregion

        #region Private

        private bool IsXRDeviceConnected()
        {
            XRGeneralSettings settings = XRGeneralSettings.Instance;

            if (settings == null)
            {
                return false;
            }

            XRManagerSettings managerSettings = settings.Manager;

            if (managerSettings == null)
            {
                return false;
            }

            if (m_LoaderStartCoroutine != null)
            {
                return false;
            }

            XRLoader loader = managerSettings.activeLoader;

            if (loader == null)
            {
                return false;
            }

            return true;
        }

        private void StartXR()
        {
            if (m_LoaderStartCoroutine != null)
                return;

            m_LoaderStartCoroutine = StartCoroutine(StartXRCoroutine());
        }

        private void StopXR()
        {
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                Debug.Log("Stopping XR...");
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }

        private IEnumerator SwitchCameras()
        {
            bool EnableAvatar = false;

            switch (m_CurrentMode)
            {
                case ViewModes.Vr:
                    m_PlayerFollowCamera.SetActive(false);
                    m_AvatarCamera.gameObject.SetActive(false);

                    EnableAvatar = false;

                    break;

                case ViewModes.FirstPerson:
                    m_AvatarCamera.gameObject.SetActive(true);
                    m_PlayerFollowCamera.SetActive(true);
                    m_AvatarGO.SetActive(true);
                    m_AvatarCamera.cullingMask = m_FirstPersonLayerMask;

                    EnableAvatar = true;

                    break;

                case ViewModes.ThirdPerson:
                default:
                    m_AvatarCamera.gameObject.SetActive(true);
                    m_PlayerFollowCamera.SetActive(true);
                    m_AvatarGO.SetActive(true);
                    m_AvatarCamera.cullingMask = m_ThirdPersonLayerMask;

                    EnableAvatar = true;

                    break;
            }

            m_XrOrigin.SetActive(!EnableAvatar);
            m_AvatarCapsuleCollider.enabled = !EnableAvatar;

            yield return ResetInput(EnableAvatar);
        }

        private IEnumerator ResetInput(bool enableAvatar)
        {
            AvatarController avatarController = m_AvatarGO.GetComponent<AvatarController>();
            PlayerInput playerInput = m_AvatarGO.GetComponent<PlayerInput>();
            XRUIInputModule uiInputModule = FindAnyObjectByType<XRUIInputModule>();
            InputActionManager inputActionManager = FindAnyObjectByType<InputActionManager>();

            if ( uiInputModule != null )
                uiInputModule.enabled = false;

            if ( inputActionManager != null )
                inputActionManager.enabled = false;

            if ( enableAvatar )
            {
                avatarController.enabled = false;
                playerInput.enabled = false;

                yield return null;
                yield return null;

                avatarController.enabled = true;
                playerInput.enabled = true;
            }
            else
            {
                avatarController.enabled = true;
                playerInput.enabled = true;

                yield return null;
                yield return null;

                avatarController.enabled = false;
                playerInput.enabled = false;
            }

            if ( uiInputModule != null )
                uiInputModule.enabled = true;

            if ( inputActionManager != null )
                inputActionManager.enabled = true;
        }
        #endregion
    }

}
