using UnityEngine;
using UnityEngine.InputSystem;
using VRWeb.Managers;
using VRWeb.User;

namespace VRWeb.Avatar
{
	public class AvatarPositioner : MonoBehaviour
    {
        [SerializeField]
        private Camera m_AvatarCamera;

        [SerializeField]
        private LayerMask m_GroundMask;

        [SerializeField]
        private float m_JumpForce = 7.0f;

        [SerializeField]
        private float m_MaxFallingSeconds = 3.0f;

        [SerializeField]
        private float m_MouseDeadzone = 0.01f;

        [SerializeField]
        private InputActionReference m_MoveActionReference;

        [SerializeField]
        private InputActionReference m_LookActionReference;

        [SerializeField]
        private InputActionReference m_SprintActionReference;

        [SerializeField]
        private InputActionReference m_JumpActionReference;

        [SerializeField]
        private InputActionReference m_EscapeKeyAction;

        private Transform m_Avatar = null;

        private Rigidbody m_RigidBody = null;
        private bool m_IsAnalogControl;
        private Animator m_Animator = null;
        private float m_FallingTimer = 0;

        private float MOVE_SPEED => AvatarSettings.global.AvatarMoveSpeed;
        private float LATERAL_SPEED => AvatarSettings.global.AvatarLateralSpeed;

        private float REVERSE_SPEED => AvatarSettings.global.AvatarReverseSpeed;

        private float ROTATE_SPEED => AvatarSettings.global.AvatarRotationSpeed;

        private float CAM_TILT_SPEED => AvatarSettings.global.AvatarCameraTiltSpeed;

        private float SPRINT_BOOST => AvatarSettings.global.AvatarSprintBoost;

        private const string STR_PARAM_WALK_BACK = "WalkingBackwards";
        private const string STR_PARAM_SPEED = "Speed";
        private const string STR_PARAM_JUMP = "Jump";
        private const string STR_PARAM_FALL = "FreeFall";
        private const string STR_PARAM_GROUNDED = "Grounded";
        private const string STR_PARAM_STEPPING_LEFT = "SteppingLeft";
        private const string STR_PARAM_STEPPING_RIGHT = "SteppingRight";

        private int PARAM_WALK_BACK;
        private int PARAM_SPEED;
        private int PARAM_JUMP;
        private int PARAM_FALL;
        private int PARAM_GROUNDED;
        private int PARAM_STEPPING_LEFT;
        private int PARAM_STEPPING_RIGHT;

        private float m_RotationSpeed = 0.0f;
        private float m_CamTiltSpeed = 0.0f;
        private bool m_IsSprinting = false;
        private bool m_IsGrounded = false;
        private bool m_DeviceUpdateIsDisabled = false;

        private Vector3 m_LastGroundedPosition = Vector3.zero;
        private string m_LastGroundedUrl;

        private void OnEnable()
        {
            m_JumpActionReference.action.started += OnJump;
            m_LookActionReference.action.performed += OnLook;
            
            //m_EscapeKeyAction.action.started += OnEscapeKeyDown;
            m_DeviceUpdateIsDisabled = false;

            PARAM_SPEED = Animator.StringToHash( STR_PARAM_SPEED );
            PARAM_WALK_BACK = Animator.StringToHash( STR_PARAM_WALK_BACK );
            PARAM_JUMP = Animator.StringToHash( STR_PARAM_JUMP );
            PARAM_GROUNDED = Animator.StringToHash( STR_PARAM_GROUNDED );
            PARAM_FALL = Animator.StringToHash( STR_PARAM_FALL );

            PARAM_STEPPING_LEFT = Animator.StringToHash(STR_PARAM_STEPPING_LEFT );
            PARAM_STEPPING_RIGHT = Animator.StringToHash(STR_PARAM_STEPPING_RIGHT );

            m_RigidBody = gameObject.GetComponent < Rigidbody >();
        }

        private void OnDisable()
        {
            m_JumpActionReference.action.started -= OnJump;
            m_LookActionReference.action.performed -= OnLook;
            //m_EscapeKeyAction.action.started -= OnEscapeKeyDown;
        }

        public Vector3 LastGroundedPosition
        {
            get
            {
                if ( m_LastGroundedUrl == HopperRoot.Get<PortalManager>().CurrentVrmlUrl )
                    return m_LastGroundedPosition;

                return Vector3.zero;
            }
        }

        private void HandleWalk()
        {
            if ( m_DeviceUpdateIsDisabled )
                return;

            Vector2 movement = m_MoveActionReference.ToInputAction().ReadValue<Vector2>();

            float deltaTime = Time.deltaTime;
            
            Vector3 avatarPosition = m_Avatar.position;
            Vector3 targetPosition = Vector3.zero;

            m_IsSprinting = m_SprintActionReference.action.IsPressed();

            if ( movement.y != 0.0f )
            {
                float speed = 0;
                if ( movement.y > 0 )
                {
                    speed = movement.y * ( MOVE_SPEED * ( m_IsSprinting ? SPRINT_BOOST : 1 ) );

                    m_Animator.SetFloat( PARAM_SPEED, speed );
                    m_Animator.SetBool( PARAM_WALK_BACK, false );
                }
                else if ( movement.y < 0 )
                {
                    speed = movement.y * REVERSE_SPEED;

                    m_Animator.SetFloat( PARAM_SPEED, 0 );
                    m_Animator.SetBool( PARAM_WALK_BACK, true );
                }

                targetPosition += m_Avatar.forward * (speed * deltaTime);
            }
            else
            {
                m_Animator.SetFloat( PARAM_SPEED, 0 );
                m_Animator.SetBool( PARAM_WALK_BACK, false );
            }

            if ( movement.x != 0.0f )
            {
                Debug.Log( $"LATERAL: {movement.x}" );

                float speed = movement.x * LATERAL_SPEED;
                Vector3 avatarRight = m_Avatar.right * speed;

                targetPosition += avatarRight * deltaTime;
                if ( movement.x > 0 )
                {
                    m_Animator.SetTrigger(PARAM_STEPPING_RIGHT);
                }
                else if ( movement.x < 0 )
                {
                    m_Animator.SetTrigger(PARAM_STEPPING_LEFT);
                }
            }
            else
            {
                //m_Animator.SetBool( PARAM_STEPPING_RIGHT, false );
                //m_Animator.SetBool( PARAM_STEPPING_LEFT, false );
            }

            targetPosition += avatarPosition;

            RaycastHit hitInfo = new RaycastHit();
            Ray ray = new Ray( targetPosition + Vector3.up * 0.5f, Vector3.down );

            bool hitSomething = Physics.Raycast(
                ray.origin,
                ray.direction,
                out hitInfo,
                3f,
                m_GroundMask,
                QueryTriggerInteraction.Ignore );

            targetPosition.y = AdjustHeightFromToPosition( avatarPosition.y, targetPosition.y );

            if ( targetPosition != avatarPosition )
                m_RigidBody.MovePosition( targetPosition );

            GroundCheck( hitSomething, hitInfo, ray );
        }

        private void HandleLook()
        {
            if ( m_DeviceUpdateIsDisabled )
                return;

            float deltaTime = Time.deltaTime;

            if ( Mathf.Abs( m_RotationSpeed ) > m_MouseDeadzone )
            {
                Vector3 rot = m_Avatar.rotation.eulerAngles;
                rot.y += m_RotationSpeed * ROTATE_SPEED;
                Quaternion quat = Quaternion.identity;
                quat.eulerAngles = rot;
                m_RigidBody.MoveRotation( quat );
            }

            if ( Mathf.Abs( m_CamTiltSpeed ) > m_MouseDeadzone )
            {
                Vector3 elevatedAvatarPosition = m_Avatar.position + Vector3.up * 1.7f;

                m_AvatarCamera.transform.RotateAround(
                    elevatedAvatarPosition,
                    m_AvatarCamera.transform.right,
                    m_CamTiltSpeed * CAM_TILT_SPEED );

                float tiltAngle = Vector3.SignedAngle(
                    Vector3.up,
                    m_AvatarCamera.transform.up,
                    m_AvatarCamera.transform.right );

                if ( tiltAngle > 100 || tiltAngle < -50 )
                {
                    m_AvatarCamera.transform.RotateAround(
                        elevatedAvatarPosition,
                        m_AvatarCamera.transform.right,
                        -m_CamTiltSpeed * CAM_TILT_SPEED );
                }
            }
        }

        private void Update()
        {
            if ( UserSettings.global == null )
                UserSettings.global = new UserSettings();

            if ( m_Avatar == null || !m_Animator.gameObject.activeSelf )
                return;

            HandleWalk();
            HandleLook();
        }

        private float AdjustHeightFromToPosition( float from, float to )
        {
            if (Mathf.Abs( to - from ) < 0.3f)
                return to;

            return from;
        }

        private void GroundCheck(bool hitSomething, RaycastHit hitInfo, Ray ray )
        {
            if ( !m_IsGrounded )
            {
                if ( hitSomething )
                {
                    float dist = hitInfo.distance - 3f;

                    if ( dist < 0.1f )
                    {
                        m_IsGrounded = true;
                        m_Animator.SetBool( PARAM_GROUNDED, true );
                        m_LastGroundedUrl = HopperRoot.Get<PortalManager>().CurrentVrmlUrl;
                    }
                }
                else if ( m_FallingTimer <= 0 )
                {
                    m_FallingTimer = 0;

                    Vector3 pos;

                    if ( HopperRoot.Get<PortalManager>().CurrentVrmlUrl == m_LastGroundedUrl )
                    {
                        pos = m_LastGroundedPosition;
                    }
                    else
                    {
                        pos = m_RigidBody.transform.position;
                    }

                    pos.y += 25;

                    hitSomething = Physics.Raycast(
                        pos,
                        ray.direction,
                        out hitInfo,
                        25.0f,
                        m_GroundMask,
                        QueryTriggerInteraction.Ignore );

                    m_RigidBody.MovePosition( hitInfo.point );
                    m_IsGrounded = true;

                    m_LastGroundedUrl = HopperRoot.Get<PortalManager>().CurrentVrmlUrl;
                    m_Animator.SetBool( PARAM_GROUNDED, true );
                }
                else
                {
                    m_FallingTimer -= Time.deltaTime;
                }
            }
            else // is grounded
            {
                if ( !hitSomething )
                {
                    m_IsGrounded = false;
                    m_Animator.SetBool( PARAM_GROUNDED, false );
                    m_Animator.SetTrigger( PARAM_FALL );
                    m_FallingTimer = m_MaxFallingSeconds;
                }
            }

            if ( m_IsGrounded )
            {
                m_LastGroundedPosition = m_RigidBody.position;
            }
        }

        public void OnEscapeKeyDown( InputAction.CallbackContext context )
        {
            m_DeviceUpdateIsDisabled = !m_DeviceUpdateIsDisabled;
            Debug.Log( $"LookIsDisabled = {m_DeviceUpdateIsDisabled}" );
        }

        public void OnLook( InputAction.CallbackContext context )
        {
            if ( m_Avatar == null )
                return;

            Vector2 mouseCoord = context.action.ReadValue < Vector2 >();
            m_RotationSpeed = 10f * mouseCoord.x / Screen.width;
            m_CamTiltSpeed = 0.5f * mouseCoord.y / Screen.height;
        }

        public void OnSprint( InputAction.CallbackContext context )
        {
            if ( m_Avatar == null || m_RigidBody == null )
                return;

            m_IsSprinting = context.action.IsPressed();
        }

        public void OnJump( InputAction.CallbackContext context )
        {
            if ( context.action.WasPressedThisFrame() && m_IsGrounded )
            {
                m_IsGrounded = false;
                m_Animator.SetBool( PARAM_GROUNDED, m_IsGrounded );
                m_Animator.SetTrigger( PARAM_JUMP );
                m_RigidBody.AddRelativeForce( Vector3.up * m_JumpForce, ForceMode.VelocityChange );
            }
        }

        public void SetAvatar( Transform avatar )
        {
            m_Avatar = avatar.parent;
            m_Animator = avatar.GetComponentInChildren < Animator >();

            if ( m_Animator == null || !m_Animator.isActiveAndEnabled )
            {
                return;
            }

            m_Animator.speed = 1.0f;
            m_Animator.SetFloat( PARAM_SPEED, 1.0f );
        }
    }

}