using Unity.XR.CoreUtils;
using UnityEngine;
using VRWeb.Avatar;
using VRWeb.Creator;
using VRWeb.Managers;

namespace VRWeb.Rig
{
	public class RigPositioner : HopperManagerMonoBehaviour <RigPositioner>, IRigPositioner
    {
        [SerializeField]
        private Transform m_Head = null;

        [SerializeField]
        private Transform m_RigTransform;

        [SerializeField]
        private XROrigin m_XROrigin;

        public Transform HeadTransform => m_Head;
        private ViewModeSwitcher m_ViewModeSwitcher => HopperRoot.Get < ViewModeSwitcher >();
        
        private Vector3 m_EntryPoint;

        private void Awake()
        {
            RegisterManager();
        }

        private void Update()
        {

        }

        public void OnHeadCollision( Collision collision, CollisionForwarder.CollisionType type )
        {
            if ( type == CollisionForwarder.CollisionType.enter )
                m_EntryPoint = HeadTransform.position;
            else
            {
                Vector3 offset = HeadTransform.position - m_EntryPoint;
                offset.y = 0;
                m_RigTransform.position -= offset * 0.1f;
            }
        }

        // Private functions
        public void SetVrPanelPosDir(float zDistance, float yDistance, Transform panelTransform, bool clearChildren = true)
        {
            Vector3 forwardGrounded = HeadTransform.forward;
            forwardGrounded.y = 0;
            forwardGrounded.Normalize();
            panelTransform.position = HeadTransform.position + forwardGrounded * zDistance + Vector3.up * yDistance;
            Vector3 lookPos = HeadTransform.position;
            lookPos.y = panelTransform.position.y;
            panelTransform.LookAt( lookPos );

            if (!clearChildren)
                return;

            for ( int i = panelTransform.childCount -1; i >= 0; i--)
            {
                Transform child = panelTransform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        public void SetPosDirFromAvatarToRig()
        {
            m_XROrigin.transform.position = UserRig.Instance.AvatarTransform.position;
            m_XROrigin.transform.forward = UserRig.Instance.AvatarTransform.forward;
        }

        public void SetPosDirFromRigToAvatar()
        {
            Vector3 headPos = m_Head.position;
            Vector3 headForward = m_Head.forward;
            headPos.y = m_XROrigin.transform.position.y;
            headForward.y = 0;

            UserRig.Instance.AvatarTransform.parent.position = headPos;
            UserRig.Instance.AvatarTransform.parent.forward = headForward.normalized;
        }

        public void SetUserPositionAndForwardDirection( Vector3 pos, Vector3 forward )
        {
            if ( m_ViewModeSwitcher.IsInVrMode)
            {
                m_XROrigin.transform.position = pos;
                m_XROrigin.transform.forward = forward;
            }
            else
            {
                CharacterController cc = UserRig.Instance.AvatarTransform.gameObject.GetComponent <CharacterController >();
                cc.enabled = false;

                UserRig.Instance.AvatarTransform.position = pos;
                UserRig.Instance.AvatarTransform.forward = forward;

                AvatarController ac = FindAnyObjectByType < AvatarController >();
                ac.Grounded = true;
                cc.enabled = true;
            }
        }
    }
}