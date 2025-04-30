using UnityEngine;
using UnityEngine.UI;

namespace VRWeb.UI
{
	[RequireComponent(typeof(BoxCollider))]
    public class UIScrollbarVRHelper : MonoBehaviour
    {
        [SerializeField]
        private Scrollbar m_Scrollbar;


        private BoxCollider m_ScrollbarCollider;
        private Scrollbar.Direction m_Direction;
        private Vector2 m_Pivot;
        private bool IsHorizontal =>
            m_Direction == Scrollbar.Direction.LeftToRight || m_Direction == Scrollbar.Direction.RightToLeft;

        void Start()
        {
            m_ScrollbarCollider = GetComponent<BoxCollider>();
            m_Direction = m_Scrollbar.direction;
            m_Pivot = GetComponent<RectTransform>().pivot;

            //for ( int i = 0; i < 100; i++ )
            //    Debug.LogWarning( "just testing a long logfile" );
        }

        private void OnTriggerStay(Collider other)
        {
            if (!m_Scrollbar.interactable)
                return;

            Vector3 hitPoint = other.transform.position;
            Vector3 hitOnScrollbar = transform.InverseTransformPoint(hitPoint);
            float hitValue = 0;

            if (IsHorizontal)
            {
                if (m_Direction == Scrollbar.Direction.LeftToRight)
                    hitValue = hitOnScrollbar.x / m_ScrollbarCollider.size.x;
                else
                    hitValue = 1 - (hitOnScrollbar.x / m_ScrollbarCollider.size.x);

                hitValue -= m_Pivot.x;
            }
            else
            {
                if (m_Direction == Scrollbar.Direction.TopToBottom)
                    hitValue = 1 - (hitOnScrollbar.y / m_ScrollbarCollider.size.y);
                else
                    hitValue = hitOnScrollbar.y / m_ScrollbarCollider.size.y;

                hitValue -= m_Pivot.y;
            }

            m_Scrollbar.value = Mathf.Clamp01(hitValue);
        }
    }
}