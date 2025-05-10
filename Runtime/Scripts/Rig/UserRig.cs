using Nuro.Processes;
using Nuro.VRWeb.MultiUser;
using Nuro.VRWeb.NetworkMessages;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Avatar;
using VRWeb.Developer;
using VRWeb.Events;
using VRWeb.Managers;
using VRWeb.Processes;
using VRWeb.User;
using VRWeb.Utils;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace VRWeb.Rig
{
	public class UserRig : SingletonMonoBehaviour < UserRig >
    {
        [Header( "Network" )]
        public NetworkManager m_NetworkManager;

        [Header( "Body Parts:" )]
        [SerializeField]
        private Transform m_HeadTransform;

        [SerializeField]
        private GameObject m_LeftHand = null;

        [SerializeField]
        private GameObject m_RightHand = null;

        [SerializeField]
        private LayerMask m_GroundMask;

        [SerializeField]
        private GameObject m_DefaultAvatar;

        [SerializeField]
        private XROrigin m_Origin;

        [SerializeField]
        private GameObject m_External;

        private AvatarController m_AvatarController;

        public Queue < AnimationInformation > AnimationInfoQueue => m_AnimationInfoQueue;

        // player
        private Queue < AnimationInformation > m_AnimationInfoQueue = new Queue < AnimationInformation >();

        // Getters
        public RigPositioner RigPositioner => m_RigPositioner;

        public ViewModeSwitcher ViewModeSwitcher => m_ViewModeSwitcher;

        public Transform HeadTransform => m_HeadTransform;

        public Transform Origin => m_Origin.transform;

        public GameObject LeftHand => m_LeftHand;

        public GameObject RightHand => m_RightHand;

        public GameObject CurrentAvatar => m_CurrentAvatar != null ? m_CurrentAvatar : LoadDefaultAvatar();

        public IAvatarIK CurrentAvatarIK => m_AvatarIk.ActiveAvatarIK;

        public Transform AvatarTransform => CurrentAvatar.transform.parent;

        public AvatarController AvatarController => m_AvatarController;

        public Vector3 LastGroundedPosition => m_LastGroundedPosition;

		public Vector3 LastGroundedForward => m_LastGroundedForward;

        // Private variables
        private RigPositioner m_RigPositioner = null;
        private ViewModeSwitcher m_ViewModeSwitcher = null;
        private Vector3 m_LastGroundedPosition = Vector3.zero;
		private Vector3 m_LastGroundedForward = Vector3.forward;
        private GameObject m_CurrentAvatar;
        private DownloadAndCacheAssetBundleProcess m_LoadAssetBundleProcess = null;
        private AvatarIKController m_AvatarIk;

        // Awake function
        void Awake()
        {
            RegisterInstanceOrDestroy( this );
            
            m_ViewModeSwitcher = GetComponent < ViewModeSwitcher >();

            //Transform watch = m_LeftHand.transform.Find( "WatchParent" );
            //watch.localPosition = UserPreferences.global.LeftWatchLocalPosition;
            //watch.localRotation = UserPreferences.global.LeftWatchLocalRotation;
            //
            //if ( watch.localPosition == Vector3.zero )
            //    watch.gameObject.SetActive( false );
            //
            //Transform finger = m_LeftHand.transform.Find( "IndexFinger" );
            //finger.localPosition = UserPreferences.global.LeftFingerLocalPosition;
            //finger.localRotation = UserPreferences.global.LeftFingerLocalRotation;
            //
            //watch = m_RightHand.transform.Find( "WatchParent" );
            //watch.localPosition = UserPreferences.global.RightWatchLocalPosition;
            //watch.localRotation = UserPreferences.global.RightWatchLocalRotation;
            //
            //if ( watch.localPosition == Vector3.zero )
            //    watch.gameObject.SetActive( false );
            //
            //finger = m_RightHand.transform.Find( "IndexFinger" );
            //finger.localPosition = UserPreferences.global.RightFingerLocalPosition;
            //finger.localRotation = UserPreferences.global.RightFingerLocalRotation;

            m_RigPositioner = GetComponentInChildren < RigPositioner >( true );
            m_AvatarController = GetComponentInChildren < AvatarController >( true );

            m_AvatarIk = m_AvatarController.GetComponent< AvatarIKController >();
        }

        void Start()
        {
            StartCoroutine( SendLocalPlayerData() );
        }

        public void LoadAvatar()
        {
            StartCoroutine( LoadAvatarCoroutine() );
        }

        public void OnToolbarExit()
        {
            ApplicationHelper.CloseApplication();
        }

        public void OnEnterScene( SceneLoadedEvent.LoadStatus loadStatus )
        {
            if ( CurrentAvatar != null )
            {
                if ( loadStatus == SceneLoadedEvent.LoadStatus.beginLoad )
                {
                    CurrentAvatar.SetActive(false);
                    m_External.SetActive( false );
                }
                else
                {
                    m_External.SetActive( true );

                    CurrentAvatar.SetActive(true);
                    Animator animatorController = CurrentAvatar.transform.parent.GetComponent<Animator>();

                    StartCoroutine(SetRuntimeAnimatorController(animatorController.runtimeAnimatorController));
                    animatorController.runtimeAnimatorController = null;
                }
            }

        }

        // Unity Event functions
        private void Update()
        {
            Scene s = SceneManager.GetActiveScene();

            if ( s.buildIndex == 0 )
                return;

            if (m_ViewModeSwitcher.IsInVrMode)
            {
                RaycastHit hitInfo = new RaycastHit();
                Ray ray = new Ray( m_HeadTransform.position, Vector3.down );

                bool hitSomething = Physics.Raycast(
                    ray.origin,
                    ray.direction,
                    out hitInfo,
                    1.0f,
                    m_GroundMask,
                    QueryTriggerInteraction.Ignore );

                if (hitSomething)
                {
                    m_LastGroundedPosition = hitInfo.point;
                    m_LastGroundedForward = m_HeadTransform.forward;
                }            }
            else
            {
                m_LastGroundedPosition = m_AvatarController.LastGroundedPosition;
                m_LastGroundedForward = m_AvatarController.LastGroundedForward;
            }

			/*
                        if ( hitSomething )
                        {
                            float absDistance = Mathf.Abs( hitInfo.distance - m_HeadTransform.localPosition.y );
                            if ( absDistance < 10.0  && absDistance > 0.02f )
                            {
                                Vector3 pos = m_HeadTransform.position;
                                pos.y = hitInfo.point.y;
                                m_RigPositioner.SetUserPositionAndForwardDirection( pos, m_HeadTransform.forward );
                            }
                        }
                        else
                        {
                            Vector3 pos3 = m_LastGroundedPosition;
                            pos3.y += 100;

                            hitSomething = Physics.Raycast(
                                pos3,
                                ray.direction,
                                out hitInfo,
                                1000.0f,
                                m_GroundMask,
                                QueryTriggerInteraction.Ignore );

                            if ( hitSomething )
                            {
                                Vector3 pos1 = m_LastGroundedPosition;
                                pos1.y = hitInfo.point.y;
                                m_Origin.MoveCameraToWorldLocation( pos1 );
                            }
                            else
                            {
                                PortalTarget target = FindObjectOfType < PortalTarget >();
                                if ( target != null )
                                    m_Origin.MoveCameraToWorldLocation( target.transform.position );
                                else
                                    m_Origin.MoveCameraToWorldLocation( Vector3.zero);

                            }
                        }

                        m_LastGroundedUrl = PortalManagerV2.Instance.LastVisitedUrl;
                        Vector3 pos2 = m_HeadTransform.position;
                        pos2.y = transform.position.y;
                        m_LastGroundedPosition = pos2;
            */
		}

		// Private functions
		private IEnumerator LoadAvatarCoroutine()
        {
            if ( string.IsNullOrEmpty( AvatarSettings.global.PrivateAvatarUri ) )
            {
                if ( AvatarSettings.global.PrivateAvatarPrefabName == AvatarSettings.AVATAR_2_0_NAME )
                {
                    LoadDefaultAvatar();
                    yield break;
                }
            }

            if ( AvatarSettings.global != null &&
                 !string.IsNullOrEmpty( AvatarSettings.global.PrivateAvatarUri ) &&
                 !string.IsNullOrEmpty( AvatarSettings.global.PrivateAvatarPrefabName ) )
            {
               
                yield return LoadAvatarCoroutine(
                    HopperRoot.Get < UrlMapper >().MappedUrl( AvatarSettings.global.PrivateAvatarUri ),
                    AvatarSettings.global.PrivateAvatarPrefabName );
            }
        }

        public IEnumerator LoadAvatarCoroutine( string resourceUri, string avatarPrefabName )
        {
            string[] bundleNames = resourceUri.Split( new char[] { '/', '\\' } );
            string bundleName = bundleNames.Last();

            if ( m_CurrentAvatar == null )
            {
                m_CurrentAvatar = m_DefaultAvatar;
            }
            else
            {
                AvatarIKController avatarIkController = m_CurrentAvatar.GetComponent < AvatarIKController >();
                if (avatarIkController != null)
                    avatarIkController.UnbindAvatarFromIK();
            }

            m_CurrentAvatar.SetActive( false );

            m_LoadAssetBundleProcess =
                HopperRoot.Get < AssetBundleManager >().CreateDownloadAndCacheAssetBundleProcess( resourceUri, 1 );

            m_LoadAssetBundleProcess.Start();

            yield return new WaitUntil( m_LoadAssetBundleProcess.IsFinished );

            if ( m_LoadAssetBundleProcess.Status != Process.ProcessStatus.Succeeded )
            {
                LoadDefaultAvatar();

                yield break;
            }

            GameObject avatarPrefab = m_LoadAssetBundleProcess.Bundle.LoadAsset < GameObject >( avatarPrefabName );

            if ( !avatarPrefab )
            {
                m_CurrentAvatar.SetActive( true );

                yield break;
            }

            Vector3 pos = m_CurrentAvatar.transform.position;
            Quaternion rot = m_CurrentAvatar.transform.rotation;
            GameObject newAvatar = Instantiate( avatarPrefab, m_CurrentAvatar.transform.parent );
            
            newAvatar.transform.position = pos;
            newAvatar.transform.rotation = rot;

            if ( m_CurrentAvatar != m_DefaultAvatar )
            {
                Destroy( m_CurrentAvatar );
            }
            else
            {
                m_CurrentAvatar.SetActive( false );
            }

            m_CurrentAvatar = newAvatar;

            if (m_NetworkManager.enabled)
                m_NetworkManager.SendChangeAvatarMessage(resourceUri, avatarPrefabName);
            
            m_AvatarController.SetupAvatar( m_CurrentAvatar );
        }

        public GameObject LoadDefaultAvatar()
        {
            GameObject defaultAvatar = m_DefaultAvatar;

            if ( m_CurrentAvatar == defaultAvatar )
                return m_CurrentAvatar;

            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if ( m_CurrentAvatar != null )
            {
                pos = m_CurrentAvatar.transform.position;
                rot = m_CurrentAvatar.transform.rotation;

                Destroy( m_CurrentAvatar );
            }

            m_CurrentAvatar = defaultAvatar;
            m_CurrentAvatar.SetActive( false);

            if ( m_NetworkManager != null && m_NetworkManager.enabled && m_NetworkManager.IsConnected)
                m_NetworkManager.SendChangeAvatarMessage("", AvatarSettings.AVATAR_2_0_NAME);

            AvatarTransform.position = pos;
            AvatarTransform.rotation = rot;

            m_AvatarController.SetupAvatar( m_CurrentAvatar );

            return CurrentAvatar;
        }

        public Quaternion GetHorizontalRotationQuaternion(Transform headTransform)
        {
            Quaternion fullRotation = headTransform.rotation;
    
            Vector3 forward = fullRotation * Vector3.forward;
            float angle = Mathf.Atan2(forward.x, forward.z);

            return Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
        }

        private IEnumerator SendLocalPlayerData()
        {
            if ( m_NetworkManager == null )
                yield break;

            yield return new WaitUntil( () => HopperRoot.Get < ViewModeSwitcher >() != null );

            ViewModeSwitcher viewModeSwitcher = HopperRoot.Get < ViewModeSwitcher >();
            
            while ( m_NetworkManager.IsConnected )
            {
                if ( viewModeSwitcher.IsInVrMode )
                {
                    Vector3 pos = UserRig.Instance.HeadTransform.position;
                    pos.y = UserRig.Instance.Origin.position.y;

                    m_NetworkManager.SendUserDataVr(
                        new NetworkManager.AvatarStateVr()
                        {
                            Position = pos,
                            Rotation = GetHorizontalRotationQuaternion(HeadTransform),
                            HeadPosition = HeadTransform.position,
                            HeadRotation = HeadTransform.rotation,
                            LeftHandPosition = LeftHand.transform.position,
                            LeftHandRotation = LeftHand.transform.rotation,
                            RightHandPosition = RightHand.transform.position,
                            RightHandRotation = RightHand.transform.rotation,
                            LeftFootPosition = pos,
                            LeftFootRotation = Quaternion.identity,
                            RightFootPosition = pos,
                            RightFootRotation = Quaternion.identity
                        } );
                }
                else if ( m_AvatarController.enabled && m_AvatarController.ActiveAnimator != null )
                {
                    NetworkManager.AvatarStateNonVr avatarStateNonVr = m_AvatarController.GetAvatarState();

                    if ( avatarStateNonVr != null )
                        m_NetworkManager.SendUserDataNonVr( avatarStateNonVr );
                }

                yield return new WaitForSecondsRealtime( 0.033f );
            }
        }

        private IEnumerator SetRuntimeAnimatorController(RuntimeAnimatorController controller)
        {
            yield return new WaitForSeconds(0.5f);

            Animator animatorController = CurrentAvatar.transform.parent.GetComponent<Animator>();
            animatorController.runtimeAnimatorController = controller;
        }

    }
}