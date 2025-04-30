using UnityEngine;
using UnityEngine.InputSystem;
using VRWeb.Managers;
using VRWeb.UI;

namespace VRWeb.Rig
{
	public class MenuButtonDetector : MonoBehaviour
	{
		[SerializeField]
		private InputActionAsset m_ActionAsset;
		[SerializeField]
		private InputActionReference m_MenuRef = null;

		void Start()
		{
			m_ActionAsset.Enable();
			m_MenuRef.action.started += OnMenu;
		}

		private void OnDestroy()
		{
			m_MenuRef.action.started -= OnMenu;
		}

		void OnMenu(InputAction.CallbackContext context)
		{
			HopperRoot.Get<IToolbar>()?.ToggleMenu();
		}
	}
}