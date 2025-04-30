using UnityEngine;
using VRWeb.Managers;
using VRWeb.Avatar;
using VRWeb.Rig;


namespace VRWeb.Avatar
{
	[RequireComponent(typeof(Animator))]
    public class AvatarIKController : MonoBehaviour
    {
        [SerializeField]
        private bool m_IsIKActive = false;

        [SerializeField]
        private Transform m_RightController = null;

        [SerializeField]
        private Transform m_LeftController = null;

        [SerializeField]
        private Transform m_VrCamera = null;

        private Animator m_Animator;
        private int m_PlayerLayer = 0;
        private IAvatarIK m_ActiveAvatarIK = null;

        public IAvatarIK ActiveAvatarIK => m_ActiveAvatarIK;

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_PlayerLayer = LayerMask.NameToLayer("Player");
        }

        public void BindAvatarToIK( GameObject avatarGameObject )
        {
            Renderer[] renderer = avatarGameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in renderer)
            {
                rend.gameObject.layer = m_PlayerLayer;
            }

            m_IsIKActive = true;
            m_ActiveAvatarIK = avatarGameObject.GetComponent<IAvatarIK>();

            if ( ActiveAvatarIK == null)
                m_ActiveAvatarIK = avatarGameObject.AddComponent<DefaultAvatarIK>();

            ActiveAvatarIK.BindAvatarToIK( avatarGameObject, m_Animator );
        }

        //public UnityEngine.Avatar avatar { get; }

        public void UnbindAvatarFromIK()
        {
            m_IsIKActive = false;
            m_ActiveAvatarIK.UnbindAvatarFromIK();
        }

        private void OnAnimatorIK()
        {
            if (!HopperRoot.Get<ViewModeSwitcher>().IsInVrMode || !m_IsIKActive)
                return;

            m_ActiveAvatarIK.OnUpdateAvatarIK(
                transform,
                new IAvatarIK.IkInfo()
                {
                    HeadPosition = m_VrCamera.position,
                    HeadRotation = m_VrCamera.rotation,
                    LeftHandPosition = m_LeftController.position,
                    LeftHandRotation = m_LeftController.rotation,
                    RightHandPosition = m_RightController.position,
                    RightHandRotation = m_RightController.rotation,
                    LeftFootPosition = UserRig.Instance.Origin.position,
                    LeftFootRotation = UserRig.Instance.Origin.rotation,
                    RightFootPosition = UserRig.Instance.Origin.position,
                    RightFootRotation = UserRig.Instance.Origin.rotation
                } );
        }

        [ContextMenu( "Calibrate()" )]
        public void Calibrate()
        {
            IAvatarIK.IkInfo info = new()
            {
                HeadPosition = m_VrCamera.position,
                HeadRotation = m_VrCamera.rotation,
                HipsPosition = Vector3.zero,
                HipsRotation = Quaternion.identity,
                LeftHandPosition = m_LeftController.position,
                LeftHandRotation = m_LeftController.rotation,
                RightHandPosition = m_RightController.position,
                RightHandRotation = m_RightController.rotation,
                LeftFootPosition = Vector3.zero,
                LeftFootRotation = Quaternion.identity,
                RightFootPosition = Vector3.zero,
                RightFootRotation = Quaternion.identity
            };

            m_ActiveAvatarIK.Calibrate( info );
        }

    }
}
