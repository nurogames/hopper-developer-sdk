using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using VRWeb.Managers;
using VRWeb.Rig;

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
        public bool sprint;
        public bool sit;
        public float zoom = 3;
        public CursorLockMode m_DefaultCursorLockMode = CursorLockMode.Confined;

		[Header("Movement Settings")]
		public bool analogMovement;

		public UnityEvent<float> onScrollWheel = new UnityEvent<float>();

        private void Start()
        {
			Cursor.lockState = m_DefaultCursorLockMode;
        }

        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
            LookInput(value.Get<Vector2>());
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

        public void OnSprint( InputValue value )
        {
            SprintInput( value.isPressed );
        }

        public void OnZoom( InputValue value )
        {
            ZoomInput( value.Get<Vector2>() );
        }

        public void OnSit( InputValue value )
        {
			SitInput( value.isPressed );
        }

		public void OnLockMouse(InputValue value)
        {
			Cursor.lockState = value.isPressed ? CursorLockMode.Locked : m_DefaultCursorLockMode;
        }

        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
        {
            ViewModeSwitcher vms = HopperRoot.Get<ViewModeSwitcher>();
            if ( vms == null )
                return; 

			if (vms.MovementIsEnabled)
			    look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

        public void SprintInput( bool newSprintState )
        {
            sprint = newSprintState;
        }

        public void SitInput( bool newSitState )
        {
            sit = newSitState;
        }

        public void ZoomInput( Vector2 newZoom )
        {
			zoom = newZoom.y < 0 ? -1 : newZoom.y > 0 ? 1 : 0;
            onScrollWheel?.Invoke( zoom );
        }

		private void OnApplicationFocus(bool hasFocus)
        {
            /*if ( hasFocus )
                SetCursorState( true );*/
        }

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.None;
		}
    }
}