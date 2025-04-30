using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VRWeb.UI
{
	[RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Collider))]
    public class UISpriteVRButton : MonoBehaviour
    {
        [System.Serializable]
        public class ClickEvent : UnityEvent { }

        // Constants
        const string IndexFingerTag = "IndexFinger";
        const float IndexFingerTriggerRadius = 0.005f;

        // Serialized variables
        [Header("Settings:")]
        [SerializeField] bool m_IsInteractable = true;
        [SerializeField][Range(0.01f, 0.5f)] float m_DeadZone = 0.1f;
        [SerializeField] float m_HighlightWaitTime = 0.025f;
        [SerializeField] bool m_UseDelayedOnClickInvoke = true;
        [FormerlySerializedAs("m_DelyedClickDuration")]
        [SerializeField] float m_DelayedClickDuration = 0.1f;
        [SerializeField] private bool m_UseAsToggle = false;
        [SerializeField] private bool m_IsOn = false;

        [Header("References:")]
        public Image m_TouchBaseImage = null;
        public Image m_BackgroundImage = null;
        public AudioClip m_OnClickAudioClip = null;

        [Header("Event:")]
        public ClickEvent OnClick = new ClickEvent();
        public UnityEvent<bool> OnValueChanged = new UnityEvent<bool>();


        public bool ToggleBehavior
        {
            get => m_UseAsToggle;

            set
            {
                m_UseAsToggle = value;
            }
        }

        public bool IsOn
        {
            get => m_IsOn;
            set
            {
                m_IsOn = value;

                if (m_UseAsToggle)
                {
                    m_BackgroundImage.gameObject.SetActive(m_IsOn);
                }
            }
        }

        // Private variables
        bool m_ValidEntering = true;
        bool m_IsHighlightActive = false;
        bool m_IsClicked = false;

        float m_EnterTime = 0.0f;
        Vector3 m_EnterPoint = Vector3.zero;

        Transform m_TouchBaseTransform = null;
        float m_InteractionZone = 1.0f;
        AudioSource m_AudioSource = null;

        Coroutine m_ClickCoroutine = null;


        // Getter
        public bool IsInteractable
        {
            get
            {
                return m_IsInteractable;
            }
            set
            {
                m_IsInteractable = value;
            }
        }

        // Awake function
        void Awake()
        {
            GetDependencies();
        }

        // OnEnable function
        void OnEnable()
        {
            ChangeInteractability(m_IsInteractable);
            ChangeTouchBasePosition(0);
        }

        // OnDisable function
        void OnDisable()
        {
            m_IsClicked = false;
            StopAllCoroutines();
            ActivateNormalHighlighting();
            m_ClickCoroutine = null;
        }

        // Public functions
        public void SetIsInteractive(bool isInteractive) => ChangeInteractability(isInteractive);

        // Trigger functions
        void OnTriggerEnter(Collider other)
        {
            if (!CheckIfInteractionIsValid(other))
                return;

            m_EnterPoint = other.transform.position;

            m_EnterTime = Time.time;
            float distance = CalculateDistance(other.transform);
            m_ValidEntering = distance >= m_DeadZone * m_InteractionZone * m_TouchBaseTransform.lossyScale.z;

        }

        void OnTriggerStay(Collider other)
        {
            if (!CheckIfInteractionIsValid(other))
                return;

            if (!m_ValidEntering)
                return;

            if (m_EnterTime + m_HighlightWaitTime <= Time.time)
            {
                ActivateHoverHighlighting();

                float distance = CalculateDistance(other.transform);

                if (distance <= 0.0f && !m_IsClicked)
                {
                    HandleClickMechanism();

                    if (m_UseDelayedOnClickInvoke)
                        m_ClickCoroutine = StartCoroutine(ClickCoroutine());
                    else
                        ToggleOrClickInvoke();
                }
                else if (!m_IsClicked)
                    ChangeTouchBasePosition(distance);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!CheckIfInteractionIsValid(other))
                return;

            ActivateNormalHighlighting();
            SetTouchBasePositionToDefault();

            if (m_ClickCoroutine != null)
            {

                StopCoroutine(m_ClickCoroutine);
                m_ClickCoroutine = null;
                ToggleOrClickInvoke();
            }
            else
                StartCoroutine(CheckForSwipeThroughHit(other));

            m_IsClicked = false;
        }

        private void ToggleOrClickInvoke()
        {
            if (m_UseAsToggle)
            {
                m_IsOn = !m_IsOn;
                OnValueChanged?.Invoke(m_IsOn);
                m_BackgroundImage.gameObject.SetActive(m_IsOn);
            }
            else
            {
                OnClick?.Invoke();
            }
        }

        private void GetDependencies()
        {
            if (m_BackgroundImage == null)
                m_BackgroundImage = GetComponent<Image>();

            m_InteractionZone = GetComponent<BoxCollider>().size.z;
            m_AudioSource = GetComponentInParent<AudioSource>();

            if (m_TouchBaseImage != null)
                m_TouchBaseTransform = m_TouchBaseImage.transform;
        }

        void HandleClickMechanism()
        {
            m_IsClicked = true;

            ChangeTouchBasePosition(0.0f);

            if (m_AudioSource != null && m_OnClickAudioClip != null)
            {
                m_AudioSource.clip = m_OnClickAudioClip;
                m_AudioSource.loop = false;
                m_AudioSource.Play();
            }
        }

        float CalculateDistance(Transform other)
        {
            Plane plane = new Plane(-transform.forward, transform.position);
            float distance = plane.GetDistanceToPoint(other.position) - IndexFingerTriggerRadius;
            if (distance < 0.0f)
                distance = 0.0f;
            return distance;
        }
        void ActivateNormalHighlighting()
        {
            if (!m_IsHighlightActive)
                return;

            m_IsHighlightActive = false;
        }

        void ActivateHoverHighlighting()
        {
            if (m_IsHighlightActive)
                return;

            m_IsHighlightActive = true;
        }

        void ChangeTouchBasePosition(float distance)
        {
            if (m_TouchBaseTransform != null)
            {
                Vector3 touchBaseLocalPosition = m_TouchBaseTransform.localPosition;

                float lossyScale = m_TouchBaseTransform.lossyScale.z == 0.0f
                    ? 0.00000001f
                    : m_TouchBaseTransform.lossyScale.z;

                touchBaseLocalPosition.z = (-distance); // / lossyScale
                m_TouchBaseTransform.localPosition = touchBaseLocalPosition;
            }
        }

        void SetTouchBasePositionToDefault()
        {
            if (m_TouchBaseTransform != null)
            {
                Vector3 touchBaseLocalPosition = m_TouchBaseTransform.localPosition;
                touchBaseLocalPosition.z = -m_InteractionZone * m_TouchBaseTransform.lossyScale.z;
                m_TouchBaseTransform.localPosition = touchBaseLocalPosition;
            }
        }

        void ChangeInteractability(bool value)
        {
            m_IsInteractable = value;

            if (m_BackgroundImage == null)
                m_BackgroundImage = GetComponent<Image>();

            if (m_IsInteractable)
            {
                SetTouchBasePositionToDefault();
            }
            else
            {
                ActivateNormalHighlighting();
                m_IsClicked = false;

                ChangeTouchBasePosition(0.0f);
            }
        }

        bool CheckIfInteractionIsValid(Collider other)
        {
            return
                gameObject.activeInHierarchy &&
                m_IsInteractable &&
                other.CompareTag(IndexFingerTag);
        }

        [ContextMenu("Trigger Click Event")]
        private void EditorTriggerClick()
        {
            ToggleOrClickInvoke();
        }

        // Coroutines
        IEnumerator CheckForSwipeThroughHit(Collider other)
        {
            if (m_IsClicked == false && m_ClickCoroutine == null)
            {
                Vector3 exitPoint = other.transform.position;
                Vector3 enterExitDirection = exitPoint - m_EnterPoint;

                float dotProduct = Vector3.Dot(enterExitDirection, this.transform.forward);
                float distanceToPlane = CalculateDistance(other.transform);

                if (dotProduct > 0 && distanceToPlane == 0f)
                {
                    ActivateHoverHighlighting();
                    HandleClickMechanism();
                    m_ClickCoroutine = StartCoroutine(ClickCoroutine());

                    if (m_DelayedClickDuration - 0.05f > 0)
                        yield return new WaitForSeconds(m_DelayedClickDuration - 0.05f);

                    ActivateNormalHighlighting();
                    SetTouchBasePositionToDefault();
                }

                ActivateNormalHighlighting();
                SetTouchBasePositionToDefault();

                yield return null;
            }
        }

        IEnumerator ClickCoroutine()
        {
            yield return new WaitForSecondsRealtime(m_DelayedClickDuration);

            ActivateNormalHighlighting();

            ToggleOrClickInvoke();

            m_ClickCoroutine = null;
        }
    }
}