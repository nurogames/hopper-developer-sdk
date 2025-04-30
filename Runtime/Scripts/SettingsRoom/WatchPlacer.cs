using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VRWeb.Creator;
using VRWeb.Rig;
using VRWeb.User;

namespace VRWeb.SettingsRoom
{
	public class WatchPlacer : MonoBehaviour
    {
        [Header("Input Action References")]
        [SerializeField]
        private InputActionReference m_LeftTriggerPressed = null;

        [SerializeField]
        private InputActionReference m_RightTriggerPressed = null;


        [SerializeField]
        private Transform m_WatchAnchor = null;

        [SerializeField]
        private ControllerObjectType m_ControllerObjectType;

        [SerializeField]
        private TMP_Text m_Instructions;

        [SerializeField]
        [Multiline]
        private string m_FirstInstructions = "Place Controller near this symbol and pull the trigger to " +
                                             "attach the Watch to the controller";

        [SerializeField]
        [Multiline]
        private string m_InTriggerInstructions = "Pull the controller's trigger to reposition watch. " +
                                                 "Hold the trigger and pull out controller to remove Watch";
        [SerializeField]
        [Multiline]
        private string m_InTriggerAndPressedInstructions = "repositioning...\n\nHold the trigger and pull " +
                                                           "out controller to remove Watch";

        private Transform m_RightHand = null;
        private Transform m_LeftHand = null;

        private Transform m_ObjectTransformRight = null;
        private Transform m_ObjectTransformLeft = null;
        private bool m_IsInside;

        public enum ControllerObjectType
        {
            Watch,
            IndexFinger
        }

        IEnumerator Start()
        {
            yield return new WaitUntil(() => UserRig.Instance != null);

            m_LeftHand = UserRig.Instance.LeftHand.transform;
            m_RightHand = UserRig.Instance.RightHand.transform;

            string watchName = m_ControllerObjectType == ControllerObjectType.Watch ? "WatchParent" : "IndexFinger";

            m_ObjectTransformLeft = m_LeftHand.Find(watchName);
            m_ObjectTransformRight = m_RightHand.Find(watchName);

            m_Instructions.text = m_FirstInstructions;
        }

        void OnDisable()
        {
        }

        public void OnTrigger(Collider collider, TriggerForwarder.TriggerType triggerType)
        {
            if (triggerType == TriggerForwarder.TriggerType.enter)
                m_IsInside = true;
            else if (triggerType == TriggerForwarder.TriggerType.exit)
                m_IsInside = false;

            if (m_LeftTriggerPressed.action.inProgress)
            {
                if (triggerType != TriggerForwarder.TriggerType.exit)
                {
                    m_ObjectTransformLeft.gameObject.SetActive(true);
                    m_Instructions.text = m_InTriggerAndPressedInstructions;

                    if (m_ControllerObjectType == ControllerObjectType.Watch)
                    {
                        UserPreferences.global.LeftWatchLocalPosition = m_ObjectTransformLeft.localPosition;
                        UserPreferences.global.LeftWatchLocalRotation = m_ObjectTransformLeft.localRotation;
                    }
                    else
                    {
                        UserPreferences.global.LeftFingerLocalPosition = m_ObjectTransformLeft.localPosition;
                        UserPreferences.global.LeftFingerLocalRotation = m_ObjectTransformLeft.localRotation;
                    }

                    return;
                }
                else if (m_ControllerObjectType == ControllerObjectType.Watch)
                {
                    m_ObjectTransformLeft.gameObject.SetActive(false);

                    UserPreferences.global.LeftWatchLocalPosition = Vector3.zero;
                    UserPreferences.global.LeftWatchLocalRotation = Quaternion.identity;
                }
            }

            if (m_RightTriggerPressed.action.inProgress)
            {
                if (triggerType != TriggerForwarder.TriggerType.exit)
                {
                    m_ObjectTransformRight.gameObject.SetActive(true);
                    m_Instructions.text = m_InTriggerAndPressedInstructions;

                    if (m_ControllerObjectType == ControllerObjectType.Watch)
                    {
                        UserPreferences.global.RightWatchLocalPosition = m_ObjectTransformRight.localPosition;
                        UserPreferences.global.RightWatchLocalRotation = m_ObjectTransformRight.localRotation;
                    }
                    else
                    {
                        UserPreferences.global.RightFingerLocalPosition = m_ObjectTransformRight.localPosition;
                        UserPreferences.global.RightFingerLocalRotation = m_ObjectTransformRight.localRotation;
                    }

                    return;
                }
                else if (m_ControllerObjectType == ControllerObjectType.Watch)
                {
                    m_ObjectTransformRight.gameObject.SetActive(false);

                    UserPreferences.global.RightWatchLocalPosition = Vector3.zero;
                    UserPreferences.global.RightWatchLocalRotation = Quaternion.identity;
                }
            }

            if (triggerType == TriggerForwarder.TriggerType.exit)
            {
                m_Instructions.text = m_FirstInstructions;

                Debug.Log("Save User Preferences");
                UserPreferences.Save();
            }
            else
            {
                m_Instructions.text = m_InTriggerInstructions;
            }
        }

        private void LateUpdate()
        {
            if (!m_IsInside)
                return;

            if (m_LeftTriggerPressed.action.inProgress)
            {
                m_ObjectTransformLeft.position = m_WatchAnchor.position;
                m_ObjectTransformLeft.rotation = m_WatchAnchor.rotation;
            }

            if (m_RightTriggerPressed.action.inProgress)
            {
                m_ObjectTransformRight.position = m_WatchAnchor.position;
                m_ObjectTransformRight.rotation = m_WatchAnchor.rotation;
            }
        }
    }
}