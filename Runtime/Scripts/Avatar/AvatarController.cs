using System;
using System.Collections;
using System.Collections.Generic;
//using AvatarGoVR;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Unity.Cinemachine;
using Nuro.VRWeb.MultiUser;
using Nuro.VRWeb.NetworkMessages;
using VRWeb.Tracking;
using VRWeb.Managers;
using Random = UnityEngine.Random;
using StarterAssets;
using VRWeb.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using VRWeb.Rig;
using VRWeb.User;
#if HOPPER
using VRWeb.MouseActions;
#endif

// Note: animations are called via the controller for both the character and capsule using m_Animator null checks

namespace VRWeb.Avatar
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class AvatarController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        public GameObject m_TeleportRingPrefab = null;

        [Header("Cinemachine")]
        [Tooltip("The Avatar Camera Game Object")]
        public GameObject MainCamera;

        [Tooltip("The Cinemachine Virtual Camera which follows the player")]
        public CinemachineVirtualCamera PlayerFollowCamera;

        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        private const float MAX_TELEPORT_DISTANCE = 30.0f;

        // cinemachine
        private float m_CinemachineTargetYaw;
        private float m_CinemachineTargetPitch;

        private float m_Speed;
        private float m_AnimationBlend;
        private float m_TargetRotation = 0.0f;
        private float m_RotationVelocity;
        private float m_VerticalVelocity;
        private float m_TerminalVelocity = 53.0f;

        // timeout deltatime
        private float m_JumpTimeoutDelta = 0;
        private float m_FallTimeoutDelta = 0;

        private bool m_IsSitting = false;

        private float m_CurrentZoom = 0;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput m_PlayerInput;
#endif
        private Animator m_Animator;

        // animation IDs
        private int m_AnimIDSpeed;
        private int m_AnimIDGrounded;
        private int m_AnimIDJump;
        private int m_AnimIDFreeFall;
        private int m_AnimIDMotionSpeed;
        private int m_AnimIDSit;

        private CharacterController m_CharacterController;
        private StarterAssetsInputs m_StarterAssetsInput;

        private Dictionary<int, float> m_LastSentAnimationValues = new Dictionary<int, float>();
        private const float m_AnimationThreshold = 0.01f; // Threshold for sending animation updates

        private const float _threshold = 0.01f;
        private float m_MaxFallingSeconds = 3.0f;
        private string m_LastGroundedUrl = "";
        private Vector3 m_LastGroundedPosition = Vector3.zero;
        private Vector3 m_LastGroundedForward = Vector3.forward;

        public Animator ActiveAnimator => m_Animator;
        public Vector3 LastGroundedPosition => m_LastGroundedPosition;
        public Vector3 LastGroundedForward => m_LastGroundedForward;

        private bool m_IsTryingToTeleport = false;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return m_PlayerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        public bool SetupAvatar(GameObject avatarGO)
        {
            GetUserPreferences();

            avatarGO.SetActive( true );

            m_Animator = GetComponent<Animator>();

            Animator avatarAnimator = avatarGO.GetComponent<Animator>();
            if (avatarAnimator == null)
            {
                Debug.LogWarning("The new Avatar root GameObject has no m_Animator.");

                return false;
            }

            m_Animator.avatar = avatarAnimator.avatar;
            avatarAnimator.enabled = false;

            gameObject.GetComponent<AvatarIKController>().BindAvatarToIK( avatarGO );

            AssignCinemachineTarget( avatarGO );

            return true;
        }

        private void AssignCinemachineTarget(GameObject avatarGO)
        {
            CinemachineCameraTarget = null;

            for ( int i = 0; i < avatarGO.transform.childCount; i++ )
            {
                if ( avatarGO.transform.GetChild( i ).tag == "CinemachineTarget" )
                {
                    CinemachineCameraTarget = avatarGO.transform.GetChild( i ).gameObject;

                    break;
                }
            }

            if ( CinemachineCameraTarget == null )
            {
                Debug.LogWarning(
                    $"Using default height for new Avatar '{avatarGO.name}' as camera target\nbecause it has no Camera Root (Tag: \"CinemachineTarget\")" );

                CinemachineCameraTarget = new GameObject();
                CinemachineCameraTarget.transform.SetParent( avatarGO.transform );
                Vector3 pos = new Vector3( 0, 1.6f, 0 );
                CinemachineCameraTarget.transform.localPosition = pos;
            }

            PlayerFollowCamera.Follow = CinemachineCameraTarget.transform;
        }

        private void OnEnable()
        {
            m_AnimIDSpeed = Animator.StringToHash( "Speed" );
            m_AnimIDGrounded = Animator.StringToHash( "Grounded" );
            m_AnimIDJump = Animator.StringToHash( "Jump" );
            m_AnimIDFreeFall = Animator.StringToHash( "FreeFall" );
            m_AnimIDMotionSpeed = Animator.StringToHash( "MotionSpeed" );
            m_AnimIDSit = Animator.StringToHash( "Sit" );

            m_CharacterController = GetComponent<CharacterController>();
            m_StarterAssetsInput = GetComponent<StarterAssetsInputs>();
            m_StarterAssetsInput.onScrollWheel.AddListener( SetZoom );
            SetZoom(0);

            StartCoroutine( InithandlersCoroutine() );
        }

        private IEnumerator InithandlersCoroutine()
        {
            yield return new WaitWhile( () => HopperRoot.Get<TrackHistory>() == null );
#if HOPPER
            yield return new WaitWhile( () => HopperRoot.Get<MouseHandler>() == null );

            HopperRoot.Get < MouseHandler >().onLeftMouseButtonDown.AddListener( OnTeleportMouseDown );
            HopperRoot.Get < MouseHandler >().onLeftMouseButtonUp.AddListener( OnTeleportMouseUp );
#endif
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            m_StarterAssetsInput.onScrollWheel.RemoveListener( SetZoom );

#if HOPPER
            if ( HopperRoot.Get < MouseHandler >() != null )
            {
                HopperRoot.Get < MouseHandler >().onLeftMouseButtonDown.RemoveListener( OnTeleportMouseDown );
                HopperRoot.Get < MouseHandler >().onLeftMouseButtonUp.RemoveListener( OnTeleportMouseUp );
            }
#endif
        }

        private IEnumerator Start()
        {
            m_Animator = GetComponent < Animator >();
            m_CinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            m_PlayerInput = GetComponent<PlayerInput>();

            // reset our timeouts on start
            m_JumpTimeoutDelta = JumpTimeout;
            m_FallTimeoutDelta = FallTimeout;

            yield return new WaitUntil(() => HopperRoot.Get < TrackHistory >() != null);
        }

        private void Update()
        {
            if (HopperRoot.Get <PortalManager>() == null) 
                return;

            if (m_Animator == null)
                TryGetComponent(out m_Animator);

            if ( m_Animator.runtimeAnimatorController != null && !SittingDown())
            {
                CheckTeleporter();
                JumpAndGravity();
                GroundedCheck();
                Move();
            }
        }

        private GameObject m_TeleportRingInstance = null;

        private void CheckTeleporter()
        {
#if HOPPER
            MouseTeleporter teleporter = HopperRoot.Get<MouseTeleporter>();

            if ( !m_IsTryingToTeleport )
            {
                if (m_TeleportRingInstance != null)
                {
                    Destroy(m_TeleportRingInstance);
                    m_TeleportRingInstance = null;
                }
                return;
            }

            if ( m_TeleportRingInstance == null )
            {
                m_TeleportRingInstance = Instantiate(m_TeleportRingPrefab, transform );
            }

            m_TeleportRingInstance.transform.position = teleporter.HoveredRaycastHit.point;
            Vector3 lookDir = HopperRoot.Get < MouseHandler >().ScreenToPointDirection;
            m_TeleportRingInstance.transform.forward = new Vector3( lookDir.x, 0, lookDir.z ).normalized;
#endif
        }

        public void SetZoom(float zoom)
        {
            if (HopperRoot.Get < ViewModeSwitcher >() == null || !HopperRoot.Get < ViewModeSwitcher >().MovementIsEnabled )
                return;

            Cinemachine3rdPersonFollow follow =
                PlayerFollowCamera.GetCinemachineComponent< Cinemachine3rdPersonFollow >();

            m_CurrentZoom = Mathf.Min(4, Mathf.Max(m_CurrentZoom - zoom * 0.25f, 0));
            if (follow.enabled)
            {
                follow.CameraDistance = m_CurrentZoom;
            }
        }

        public void SetZoomAbs(float distance)
        {
            Cinemachine3rdPersonFollow follow =
                PlayerFollowCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            m_CurrentZoom = distance;

            if (follow.enabled)
                follow.CameraDistance = m_CurrentZoom;
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void OnTeleportMouseDown()
        {
#if HOPPER
            MouseTeleporter teleporter = HopperRoot.Get < MouseTeleporter >();

            if ( teleporter == null || teleporter.HoveredFloor == null )
                return;

            CursorController cursorController = HopperRoot.Get < CursorController >();

            if ( cursorController.CurrentCursorMode != ICursorController.Modes.Default &&
                 cursorController.CurrentCursorMode != ICursorController.Modes.Teleport )
                return;

            m_IsTryingToTeleport = true;
            StartCoroutine( AnimateTeleportCoroutine()  );
#endif
        }

        private void OnTeleportMouseUp()
        {
            if (!m_IsTryingToTeleport)
                return;

            m_IsTryingToTeleport = false;
        }

        private IEnumerator AnimateTeleportCoroutine()
        {
#if HOPPER
            if ( !m_Animator.enabled )
                yield break;

            yield return new WaitUntil(() => m_TeleportRingInstance != null);

            float teleportDelay = 0.5f;
            double now = Time.time;

            //SetAnimationFloat( m_AnimIDSpeed, 0.01f );
            SetAnimationBool( m_AnimIDJump, true);

            MouseTeleporter teleporter = HopperRoot.Get<MouseTeleporter>();
            Transform ballTransform = m_TeleportRingInstance.transform.GetChild(1);

            Vector3 fromVector= new Vector3( 0.025f, 0.025f, 0.025f );
            float toSize = 0.47f;
            float lerpT = 0f;

            Vector3 jumpToPos = teleporter.HoveredRaycastHit.point;

            while ( m_IsTryingToTeleport && ( Time.time - now ) <= teleportDelay )
            {
                lerpT = (float)( Time.time - now ) / teleportDelay;
                ballTransform.localScale = Vector3.Lerp(
                    fromVector, 
                    new Vector3(toSize, 0.025f, toSize), 
                    lerpT);

                Vector3 diff = jumpToPos - teleporter.HoveredRaycastHit.point;
                if ( diff.sqrMagnitude > 0.1f )
                {
                    jumpToPos = teleporter.HoveredRaycastHit.point;
                    now = Time.time;
                }

                yield return null;
            }

            if ( !m_IsTryingToTeleport || teleporter.HoveredFloor == null )
            {
                m_IsTryingToTeleport = false;
                SetAnimationBool( m_AnimIDJump, false );
                SetAnimationBool( m_AnimIDFreeFall, false );
                SetAnimationBool( m_AnimIDGrounded, true );

                yield break;
            }

            Vector3 lookDir = HopperRoot.Get < MouseHandler >().ScreenToPointDirection;
            lookDir = new Vector3( lookDir.x, 0, lookDir.z ).normalized;

            m_CharacterController.enabled = false;
            m_VerticalVelocity = -Gravity * Time.deltaTime;
            transform.position = teleporter.HoveredRaycastHit.point;
            transform.forward = lookDir;
            m_CharacterController.enabled = true;

            SetAnimationBool( m_AnimIDJump, false );
            SetAnimationBool( m_AnimIDFreeFall, false );
            SetAnimationBool( m_AnimIDGrounded, true );

            m_IsTryingToTeleport = false;
#else
            yield break;    
#endif
        }

        private bool SittingDown()
        {
            if ( !HopperRoot.Get < ViewModeSwitcher >().MovementIsEnabled )
                return false;

            bool wantToSit = m_StarterAssetsInput.sit;
            
            if ( wantToSit && !m_IsSitting )
            {
                SetAnimationBool( m_AnimIDSit, true );
                m_IsSitting = true;
                m_StarterAssetsInput.jump = false;

                Debug.Log( "sit down" );

                return true;
            }
            else if ( m_IsSitting && m_StarterAssetsInput.jump )
            {
                SetAnimationBool( m_AnimIDSit, false );
                m_IsSitting = false;
                m_StarterAssetsInput.jump = false;

                Debug.Log( "stand up" );

                return true;
            }

            return m_IsSitting;
        }

        private void GroundedCheck()
        {
            bool lastValueGrounded = Grounded;

            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);


            if (Grounded)
            {
                m_LastGroundedUrl = HopperRoot.Get<PortalManager>().CurrentVrmlUrl;
                m_LastGroundedPosition = transform.position;
                m_LastGroundedForward = transform.forward;
            }

            if (lastValueGrounded != Grounded)
                SetAnimationBool(m_AnimIDGrounded, Grounded);
        }
        private void JumpAndGravity()
        {
            bool canMove = HopperRoot.Get<ViewModeSwitcher>().MovementIsEnabled;
            if ( !canMove || m_IsTryingToTeleport )
                return;

            if ( Grounded )
            {
                // reset the fall timeout timer
                m_FallTimeoutDelta = FallTimeout;

                // update m_Animator if using character
                SetAnimationBool(m_AnimIDJump, false);
                SetAnimationBool(m_AnimIDFreeFall, false);
                SetAnimationBool( m_AnimIDGrounded, true );

                // stop our velocity dropping infinitely when grounded
                if (m_VerticalVelocity < 0.0f)
                {
                    m_VerticalVelocity = -2f;
                }

                // Jump
                if (canMove && (m_StarterAssetsInput.jump && m_JumpTimeoutDelta <= 0.0f))
                {
                    m_StarterAssetsInput.jump = false;
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    m_VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update m_Animator if using character
                    SetAnimationBool( m_AnimIDGrounded, false );
                    SetAnimationBool(m_AnimIDJump, true);
                }

                // jump timeout
                if (m_JumpTimeoutDelta >= 0.0f)
                {
                    m_JumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                m_JumpTimeoutDelta = JumpTimeout;

                // fall timeout
                m_FallTimeoutDelta -= Time.deltaTime;

                if (m_FallTimeoutDelta < 0.0f)
                {
                    // update m_Animator if using character
                    SetAnimationBool(m_AnimIDFreeFall, true);

                    if (m_MaxFallingSeconds + m_FallTimeoutDelta < 0)
                    {
                        Grounded = true;
                        m_CharacterController.enabled = false;
                        SetAnimationBool(m_AnimIDFreeFall, false);
                        m_VerticalVelocity = -Gravity * Time.deltaTime;
                        if (m_LastGroundedPosition == Vector3.zero && HopperRoot.Get<TrackHistory>().TrackList.Count > 0)
                        {
                            m_LastGroundedPosition = HopperRoot.Get<TrackHistory>().TrackList[0].LastPosition;
                        }
                        transform.position = m_LastGroundedPosition;
                        m_CharacterController.enabled = true;
                    }
                }

                // if we are not grounded, do not jump
                m_StarterAssetsInput.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (m_VerticalVelocity < m_TerminalVelocity)
            {
                m_VerticalVelocity += Gravity * Time.deltaTime;
            }
        }


        private void Move()
        {
            if (!HopperRoot.Get<ViewModeSwitcher>().MovementIsEnabled)
                return;

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = m_StarterAssetsInput.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (m_StarterAssetsInput.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(m_CharacterController.velocity.x, 0.0f, m_CharacterController.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = m_StarterAssetsInput.analogMovement ? m_StarterAssetsInput.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                m_Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                m_Speed = Mathf.Round(m_Speed * 1000f) / 1000f;
            }
            else
            {
                m_Speed = targetSpeed;
            }

            m_AnimationBlend = Mathf.Lerp(m_AnimationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (m_AnimationBlend < 0.01f) m_AnimationBlend = 0f;

            // normalize input direction
            Vector3 inputDirection = new Vector3(m_StarterAssetsInput.move.x, 0.0f, m_StarterAssetsInput.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (m_StarterAssetsInput.move != Vector2.zero)
            {
                m_TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                   MainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_TargetRotation, ref m_RotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, m_TargetRotation, 0.0f) * Vector3.forward;

            // move the player
            m_CharacterController.Move(targetDirection.normalized * (m_Speed * Time.deltaTime) +
                                       new Vector3(0.0f, m_VerticalVelocity, 0.0f) * Time.deltaTime);

            // update m_Animator if using character
            SetAnimationFloat(m_AnimIDSpeed, m_AnimationBlend);
            SetAnimationFloat(m_AnimIDMotionSpeed, inputMagnitude);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(m_CharacterController.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(m_CharacterController.center), FootstepAudioVolume);
            }
        }

        private void GetUserPreferences()
        {
            UserPreferences.Load();
 
            Transform leftHand = UserRig.Instance.LeftHand.transform;
            Transform rightHand = UserRig.Instance.RightHand.transform;

            string watchName ="WatchParent";// : "IndexFinger";

            Transform objectTransformLeft = leftHand.Find( watchName );
            Transform objectTransformRight = rightHand.Find( watchName );

            if ( UserPreferences.global.RightWatchLocalPosition == Vector3.zero )
            {
                objectTransformRight.gameObject.SetActive( false );
            }
            else
            {
                objectTransformRight.localPosition = UserPreferences.global.RightWatchLocalPosition;
                objectTransformRight.localRotation = UserPreferences.global.RightWatchLocalRotation;

                objectTransformRight.gameObject.SetActive( true );
            }

            if ( UserPreferences.global.LeftWatchLocalPosition == Vector3.zero )
            {
                objectTransformLeft.gameObject.SetActive( false );
            }
            else
            {
                objectTransformLeft.localPosition = UserPreferences.global.LeftWatchLocalPosition;
                objectTransformLeft.localRotation = UserPreferences.global.LeftWatchLocalRotation;

                objectTransformLeft.gameObject.SetActive( true );
            }

            string fingerName = "IndexFinger";

            objectTransformLeft = leftHand.Find( fingerName );
            objectTransformRight = rightHand.Find( fingerName );

            objectTransformRight.localPosition = UserPreferences.global.RightFingerLocalPosition;
            objectTransformRight.localRotation = UserPreferences.global.RightFingerLocalRotation;

            objectTransformLeft.localPosition = UserPreferences.global.LeftFingerLocalPosition;
            objectTransformLeft.localRotation = UserPreferences.global.LeftFingerLocalRotation;
        }

        public NetworkManager.AvatarStateNonVr GetAvatarState()
        {
            if ( !m_Animator.enabled )
                return null;

            Queue < AnimationInformation > animationInformations = new Queue < AnimationInformation >();

            animationInformations.Enqueue(
                new AnimationInformation( m_AnimIDSpeed, m_Animator.GetFloat( m_AnimIDSpeed ) ) );

            animationInformations.Enqueue(
                new AnimationInformation( m_AnimIDMotionSpeed, m_Animator.GetFloat( m_AnimIDMotionSpeed ) ) );

            animationInformations.Enqueue(new AnimationInformation( m_AnimIDGrounded, m_Animator ));
            animationInformations.Enqueue( new AnimationInformation( m_AnimIDJump, m_Animator ));
            animationInformations.Enqueue(new AnimationInformation( m_AnimIDFreeFall, m_Animator ));
            animationInformations.Enqueue(new AnimationInformation( m_AnimIDSit, m_Animator ));

            return new NetworkManager.AvatarStateNonVr()
            {
                Position = transform.position,
                Rotation = transform.rotation,
                AnimationInformations = animationInformations
            };
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if ( m_StarterAssetsInput.look.sqrMagnitude >= _threshold && !LockCameraPosition )
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                m_CinemachineTargetYaw += m_StarterAssetsInput.look.x * deltaTimeMultiplier;
                m_CinemachineTargetPitch += m_StarterAssetsInput.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            m_CinemachineTargetYaw = ClampAngle( m_CinemachineTargetYaw, float.MinValue, float.MaxValue );
            m_CinemachineTargetPitch = ClampAngle( m_CinemachineTargetPitch, BottomClamp, TopClamp );

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                m_CinemachineTargetPitch + CameraAngleOverride,
                m_CinemachineTargetYaw,
                0.0f );
        }

        public void SetAnimationBool( int animationId, bool isActive )
        {
             if (m_Animator.enabled && m_Animator.gameObject.activeSelf)
                m_Animator.SetBool( animationId, isActive );
        }

        public void SetAnimationFloat( int animationId, float value )
        {
            if ( m_Animator.enabled && m_Animator.gameObject.activeSelf )
                m_Animator.SetFloat( animationId, value );
        }

        public void OnEnterScene( SceneLoadedEvent.LoadStatus status )
        {
            if ( m_Animator != null )
                m_Animator.enabled = !( status == SceneLoadedEvent.LoadStatus.beginLoad );
        }
    }
}
