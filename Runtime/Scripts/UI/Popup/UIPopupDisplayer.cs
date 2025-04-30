using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRWeb.Events;
using VRWeb.Managers;
using VRWeb.Rig;
using VRWeb.UI.Elements;
using VRWeb.User;
using Button = VRWeb.UI.Elements.Button;

namespace VRWeb.UI
{

	public class UIPopupDisplayer : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_DesktopOverlay = null;

        [SerializeField]
        private GameObject m_VROverlay = null;

        [SerializeField]
        private ViewModeChangedEvent m_ViewModeChangedEvent;

        // Private variables
        private GameObject m_Panel = null;

        // Private variables
        private Coroutine m_ShowCoroutine = null;
        private Coroutine m_HideCoroutine = null;

        private Popup m_CurrentPopup = null;
        private PopupStyle m_Style = null;

        private Vector3 m_VrPanelVelocity;
        private ViewModes m_LastViewMode;

        // Getter
        public bool ShowCoroutineIsRunning => m_ShowCoroutine != null;

        public bool HideCoroutineIsRunning => m_HideCoroutine != null;

        #region Unity Message Functions

        // Start function
        private void Start()
        {
            m_DesktopOverlay.SetActive(false);
        }

        public void OnViewModeChanged(ViewModes oldMode, ViewModes newMode)
        {
            if ( oldMode == newMode || m_CurrentPopup == null || m_Panel == null)
                return;

            Show(m_CurrentPopup, m_Style);
            m_LastViewMode = newMode;
        }

        private void Update()
        {
            if ( m_CurrentPopup == null 
                    || m_Panel == null 
                    || HopperRoot.Get < ViewModeSwitcher >() == null
                    || HopperRoot.Get < ViewModeSwitcher >().CurrentViewMode != ViewModes.Vr
                    || HopperRoot.Get < RigPositioner >() == null 
                    || HopperRoot.Get < RigPositioner >().HeadTransform == null )
            {
                m_VrPanelVelocity = Vector3.zero;
                return;
            }

            //Position Update

            Vector3 headToPanelDirection = m_Panel.transform.position - HopperRoot.Get < RigPositioner >().HeadTransform.position;
            headToPanelDirection.y = 0;

            m_Panel.transform.position = Vector3.SmoothDamp(
                m_Panel.transform.position,
                GetVRPanelPosition(),
                ref m_VrPanelVelocity,
                UserPreferences.global.PopupFollowSpeed);

            m_Panel.transform.forward = headToPanelDirection.normalized;
        }

        #endregion

        #region Public

        public void Hide()
        {
            if (m_HideCoroutine == null)
            {
                m_HideCoroutine = StartCoroutine(HideCoroutine());
            }
        }

        // Public function
        public void Show(Popup popup, PopupStyle defaultStyle)
        {
            m_LastViewMode = HopperRoot.Get < ViewModeSwitcher >().CurrentViewMode;
            StopShowingCurrentPopup();

            m_CurrentPopup = popup;
            m_Style = popup.Style == null ? defaultStyle : popup.Style;

            m_ShowCoroutine = StartCoroutine(ShowCoroutine());
        }

        //Todo: Müssen wir überprüfen und anpassen / verbessern vorallem wenn mehrere Popups aufeinmal auftauchen.
        public void ReplaceText(Popup popup, string title, string text)
        {
            if (m_Panel == null)
                return;

            m_CurrentPopup = popup;
            TMP_Text[] texts = m_Panel.GetComponentsInChildren<TMP_Text>();
            texts[0].text = string.IsNullOrEmpty(title) ? texts[0].text : title;
            texts[1].text = string.IsNullOrEmpty(text) ? texts[1].text : text;
        }

        #endregion

        #region Private

        private Vector3 GetVRPanelPosition()
        {
            //Position the panel 20 centimeters more down than the eye
            Vector3 panelPosition = HopperRoot.Get < RigPositioner >().HeadTransform.position -
                                    Vector3.up * UserPreferences.global.PopupPlacementHeight;

            Vector3 forwardDirection = HopperRoot.Get < RigPositioner >().HeadTransform.forward;
            forwardDirection.y = 0;

            float distance = UserPreferences.global.PopupPlacementDistance;
            panelPosition += forwardDirection.normalized * distance;

            return panelPosition;
        }

        private IEnumerator HideCoroutine()
        {
            if ( m_Panel == null )
                yield break;

            float startTime = Time.time;

            m_Panel.transform.localScale = Vector3.one;

            while (Time.time <= startTime + m_Style.PanelStyle.OpenAnimationDuration)
            {
                float progress = (Time.time - startTime) / m_Style.PanelStyle.OpenAnimationDuration;

                m_Panel.transform.localScale = Vector3.one * m_Style.PanelStyle.OpenScaleCurve.Evaluate(1.0f - progress);

                yield return null;
            }

            m_Panel.transform.localScale = Vector3.zero;

            m_DesktopOverlay.SetActive(false);
            m_VROverlay.SetActive(false);

            Destroy(m_Panel);
            m_Panel = null;

            m_CurrentPopup = null;
            m_Style = null;
            m_HideCoroutine = null;
        }

        // Coroutine functions
        private IEnumerator ShowCoroutine()
        {
            Transform parentTransform = m_DesktopOverlay.transform;
            GameObject panelPrefab = m_Style.PanelStyle.UiPrefab;
            bool vrMode = false;

            if ( HopperRoot.Get<ViewModeSwitcher>().IsInVrMode)
            {
                yield return new WaitUntil( () => HopperRoot.Get<ViewModeSwitcher>().IsXRActive() );
                vrMode = true;
                parentTransform = m_VROverlay.transform;
                panelPrefab = m_Style.PanelStyle.VrPrefab;

                m_Panel = Instantiate(panelPrefab, parentTransform);

                Vector3 panelPosition = GetVRPanelPosition();

                //Set Position
                m_Panel.transform.position = panelPosition;
                Vector3 forward = -UserRig.Instance.HeadTransform.forward;
                forward.y = 0;
                m_Panel.transform.forward = forward.normalized;
            }
            else
            {
                m_Panel = Instantiate(panelPrefab, parentTransform);
            }

            TMP_Text[] texts = m_Panel.GetComponentsInChildren<TMP_Text>();
            texts[0].text = m_CurrentPopup.Title;
            texts[1].text = m_CurrentPopup.Text;

            Transform buttonParent = m_Panel.transform.GetChild(m_Panel.transform.childCount - 1);

            foreach (Button button in m_CurrentPopup.Buttons)
            {
                ButtonStyle buttonStyle = button.Style == null ? m_Style.ButtonStyle : button.Style;

                GameObject buttonPrefab = button.IsTextButton
                    ? vrMode ? buttonStyle.VrTextButtonPrefab : buttonStyle.UiTextButtonPrefab
                    : vrMode
                        ? buttonStyle.VrIconButtonPrefab
                        : buttonStyle.UiIconButtonPrefab;

                GameObject buttonGO = Instantiate<GameObject>(buttonPrefab, buttonParent);

                if (!vrMode)
                {
                    UnityEngine.UI.Button uiButton = buttonGO.GetComponent<UnityEngine.UI.Button>();
                    uiButton.onClick.AddListener(button.Invoke);
                }
                else
                {
                    SpriteVRButton vrButton = buttonGO.GetComponent<SpriteVRButton>();
                    vrButton.OnClick.AddListener(button.Invoke);
                }

                if (button.IsTextButton)
                {
                    TMP_Text buttonLabel = buttonGO.GetComponentInChildren<TMP_Text>();
                    buttonLabel.text = button.Text;
                }
                else
                {
                    if (!vrMode)
                    {
                        Image iconImage = buttonGO.transform.GetChild(0).GetComponent<Image>();
                        iconImage.sprite = button.Icon;
                    }
                    else
                    {
                        SpriteRenderer vrRenderer = buttonGO.transform.GetChild(0).GetComponent<SpriteRenderer>();
                        vrRenderer.sprite = button.Icon;
                    }
                }
            }

            if (vrMode)
            {
                m_VROverlay.SetActive(true);
            }
            else
            {
                m_DesktopOverlay.SetActive(true);
            }

            float startTime = Time.time;

            m_Panel.transform.localScale = Vector3.zero;

            while (Time.time <= startTime + m_Style.PanelStyle.OpenAnimationDuration)
            {
                float progress = (Time.time - startTime) / m_Style.PanelStyle.OpenAnimationDuration;

                m_Panel.transform.localScale = Vector3.one * m_Style.PanelStyle.OpenScaleCurve.Evaluate(progress);

                yield return null;
            }

            m_Panel.transform.localScale = Vector3.one;

            m_ShowCoroutine = null;
        }

        // Private functions
        private void StopShowingCurrentPopup()
        {
            if (m_ShowCoroutine != null)
            {
                StopCoroutine(m_ShowCoroutine);
                m_ShowCoroutine = null;
            }

            if (m_HideCoroutine != null)
            {
                StopCoroutine(m_HideCoroutine);
                m_HideCoroutine = null;
            }

            if (m_Panel != null)
            {
                Destroy(m_Panel);
                m_Panel = null;
            }

            if (m_Style != null)
            {
                m_Style = null;
            }

            if (m_CurrentPopup != null)
            {
                m_CurrentPopup = null;
            }
        }

        #endregion
    }

}