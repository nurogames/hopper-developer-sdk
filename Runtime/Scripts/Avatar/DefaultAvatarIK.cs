using UnityEngine;

namespace VRWeb.Avatar
{
	/// <summary>
	/// This is the default implementation of a simple IK driver for avatar
	/// puppets. Place the script at the root of the puppet
	/// (where it usually has its Animator)
	/// </summary>
	public class DefaultAvatarIK : MonoBehaviour,IAvatarIK
    {
        private UnityEngine.Avatar m_Avatar = null;
        private GameObject m_LastBoundAvatar = null;
        private bool m_IsRightControllerActive = false;
        private bool m_IsLeftControllerActive = false;
        private Vector3 m_LastRightHandPosition = Vector3.zero;
        private Vector3 m_LastLeftHandPosition = Vector3.zero;
        private Animator m_Animator;
        private float m_HeadHeight = 1.75f;

        /// <summary>
        /// return the avatar belonging to this puppet's skeleton
        /// </summary>
        public UnityEngine.Avatar avatar => m_Avatar;

        /// <summary>
        /// bind to this puppet. Note that the Animator parameter is NOT the Animator at
        /// the root of the Avatar puppet!
        /// </summary>
        /// <param name="avatarGameObject"></param>
        public void BindAvatarToIK( GameObject avatarGameObject, Animator externalAnimator )
        {
            if ( m_LastBoundAvatar != null )
                UnbindAvatarFromIK();

            m_Avatar = avatarGameObject.GetComponent<Animator>().avatar;
            externalAnimator.avatar = m_Avatar;

            m_LastBoundAvatar = avatarGameObject;
            m_Animator = externalAnimator;
        }

        /// <summary>
        /// unbind from the puppet and enter passive state
        /// </summary>
        public void UnbindAvatarFromIK()
        {
            m_Animator.SetLookAtWeight( 0 );
            m_LastBoundAvatar = null;
            m_Animator = null;
        }

        /// <summary>
        /// Process position and orientation information. Will be called once
        /// per frame from Unity's OnAnimatorIK() function. Do NOT implement your
        /// own OnAnimatorIK!!
        /// </summary>
        /// <param name="ikInfo"></param>
        public void OnUpdateAvatarIK( Transform avatarTransform, IAvatarIK.IkInfo ikInfo )
        {
            if ( m_Animator == null )
            {
                return; // forgot to call BindAvatarToIK()
            }

            m_IsRightControllerActive = ikInfo.RightHandPosition != m_LastRightHandPosition;
            m_IsLeftControllerActive = ikInfo.LeftHandPosition != m_LastLeftHandPosition;

            m_LastRightHandPosition = ikInfo.RightHandPosition;
            m_LastLeftHandPosition = ikInfo.LeftHandPosition;

            m_Animator.SetLookAtWeight( 1 );
            m_Animator.SetLookAtPosition( ikInfo.HeadPosition + (ikInfo.HeadRotation * Vector3.forward ));
            
            Vector3 localHeight = ikInfo.HeadPosition - ikInfo.LeftFootPosition;
            localHeight.x = 0;
            localHeight.z = 0;
            m_Animator.transform.position = ikInfo.HeadPosition - localHeight;

            Vector3 avatarForward = m_Animator.transform.forward;
            avatarForward.y = 0;
            Vector3 camForward = ikInfo.HeadRotation * Vector3.forward;
            camForward.y = 0;
            float angle = Vector3.SignedAngle( avatarForward.normalized, camForward.normalized, Vector3.up );
            m_Animator.transform.Rotate( Vector3.up, angle * Time.deltaTime );

            if ( m_IsLeftControllerActive )
                SetHandIK( AvatarIKGoal.LeftHand, ikInfo.LeftHandPosition );

            if ( m_IsRightControllerActive )
                SetHandIK( AvatarIKGoal.RightHand, ikInfo.RightHandPosition );
        }

        private void SetHandIK( AvatarIKGoal goal, Vector3 handPosition )
        {
            if ( handPosition != Vector3.zero )
            {
                m_Animator.SetIKPositionWeight( goal, 1 );
                m_Animator.SetIKPosition( goal, handPosition );
            }
            else // stop controlling hand while the controller is not active
            {
                m_Animator.SetIKPositionWeight( goal, 0 );
            }
        }

        /// <summary>
        /// This will be called once after BindAvatarToIK() to allow for adjustments
        /// to the height and reach of the avatar puppet.
        /// This function will be called while the user stands in a T-pose or
        /// read from a settings file for this user.
        /// </summary>
        /// <param name="ikInfo"></param>
        public void Calibrate( IAvatarIK.IkInfo ikInfo )
        {
            m_HeadHeight = ikInfo.HeadPosition.y - m_Animator.transform.position.y;
        }
    }
}