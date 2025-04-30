using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VRWeb.Managers;
using VRWeb.Rig;
using VRWeb.Avatar;

namespace VRWeb.SettingsRoom
{
	public class AvatarCalibrator : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference m_LeftTriggerPressed = null;

        [SerializeField]
        private InputActionReference m_RightTriggerPressed = null;

        [SerializeField]
        private TMP_Text m_CalibrationInstructions;

        private string m_FirstInstructions = @"<B>Calibrate Avatar</B>

Please stand upright and stretch out your arms.

Press and release trigger on either controller to calibrate.";

        private string m_FinishedInstructions = @"Calibration Complete!";

        private bool m_CalibrationComplete = false;

        public void OnEnableCalibration()
        {
            if (!HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
                return;

            m_CalibrationComplete = false;

            m_CalibrationInstructions.gameObject.SetActive(true);
            m_CalibrationInstructions.text = m_FirstInstructions;

            m_LeftTriggerPressed.action.started += OnPressed;
            //m_LeftTriggerPressed.action.canceled += OnPressed;

            m_RightTriggerPressed.action.started += OnPressed;
            //m_RightTriggerPressed.action.canceled += OnPressed;
        }

        public void OnDisableCalibration()
        {
            m_CalibrationInstructions.gameObject.SetActive(false);

            m_LeftTriggerPressed.action.started -= OnPressed;
            //m_LeftTriggerPressed.action.canceled -= OnPressed;

            m_RightTriggerPressed.action.started -= OnPressed;
            //m_RightTriggerPressed.action.canceled -= OnPressed;
        }

        private void OnPressed(InputAction.CallbackContext cc)
        {
            Calibrate();
        }

        private void Calibrate()
        {
            if (m_CalibrationComplete)
                return;

            m_CalibrationInstructions.text = m_FinishedInstructions;
            m_CalibrationComplete = true;

            Transform leftHand = UserRig.Instance.LeftHand.transform;
            Transform rightHand = UserRig.Instance.RightHand.transform;
            Transform head = UserRig.Instance.HeadTransform;
            Transform avatarTransform = UserRig.Instance.CurrentAvatar.transform;

            UserRig.Instance.CurrentAvatarIK.Calibrate(
                new IAvatarIK.IkInfo()
                {
                    HeadPosition = head.position,
                    HeadRotation = head.rotation,
                    LeftHandPosition = leftHand.position,
                    LeftHandRotation = leftHand.rotation,
                    RightHandPosition = rightHand.position,
                    RightHandRotation = rightHand.rotation,
                    LeftFootPosition = avatarTransform.position,
                    LeftFootRotation = avatarTransform.rotation,
                    RightFootPosition = avatarTransform.position,
                    RightFootRotation = avatarTransform.rotation,
                });
            OnDisableCalibration();
        }
    }
}