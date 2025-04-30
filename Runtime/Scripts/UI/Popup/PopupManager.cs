using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeb.Events;
using VRWeb.Managers;

namespace VRWeb.UI
{
	public class PopupManager : HopperManagerMonoBehaviour<PopupManager>
    {
        [SerializeField]
        private UIPopupDisplayer m_Displayer = null;

        [SerializeField]
        private PopupStyle m_DefaultStyle = new PopupStyle();

        // Private variables
        private Popup m_CurrentPopup = null;

        List <Popup> m_InfoPopups = new List<Popup>();
        List <Popup> m_WarningPopups = new List<Popup>();
        List <Popup> m_ErrorPopups = new List<Popup>();

        Coroutine m_ShowCoroutine = null;
        Coroutine m_HideCoroutine = null;

        private SceneLoadedEvent m_SceneLoadedEvent;

        // Getter
        public Popup CurrentPopup => m_CurrentPopup;
        public bool IsInHideCoroutine => m_HideCoroutine != null;

        #region unity message functions

        private void Awake()
        {
            RegisterManager();
        }

        private void OnEnable()
        {
            m_SceneLoadedEvent = Resources.Load < SceneLoadedEvent >( "SceneLoadedEvent" );
            m_SceneLoadedEvent.onSceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            m_SceneLoadedEvent.onSceneLoaded -= OnSceneLoaded;
        }

        #endregion unity message functions

        #region Public functions

        public void ShowPopup( Popup popup )
        {
            if( popup == null )
                return;

            if ( m_InfoPopups.Contains( popup ) ||
                 m_WarningPopups.Contains( popup ) ||
                 m_ErrorPopups.Contains( popup ) )
                return;

            AddPopupToLists( popup );
            ShowNextPopup();
        }
        
        public void UpdatePopup( Popup popup, string title, string text )
        {
            if ( popup == null )
                return;
            m_Displayer.ReplaceText( popup, title, text );
        }

        public void RemovePopup( Popup popup )
        {
            if( popup == null )
                return;

            if ( CurrentPopup == popup )
                m_HideCoroutine = StartCoroutine( HidePopupCoroutine() );
            else
                RemovePopupFromLists( popup );
        }

        #endregion Public functions

        #region Private functions

        private void OnSceneLoaded( SceneLoadedEvent.LoadStatus status )
        {
            if ( status == SceneLoadedEvent.LoadStatus.beginLoad )
            {
                m_HideCoroutine = StartCoroutine( HidePopupCoroutine()  ) ;
            }
        }

        void ShowNextPopup()
        {
            Popup popupToShow = null;

            if ( m_CurrentPopup == null )
            {
                popupToShow = GetNextPopup();
                if( popupToShow != null )
                    RemovePopupFromLists(popupToShow);
            }
            else
            {
                Popup nextPopup = GetNextPopup();
                if ( nextPopup != null && ( byte )m_CurrentPopup.Priority < ( byte )nextPopup.Priority )
                {
                    popupToShow = nextPopup;
                    RemovePopupFromLists(popupToShow);
                }
            }

            if ( popupToShow != null )
            {
                if ( m_ShowCoroutine != null )
                {
                    StopCoroutine( m_ShowCoroutine );
                    m_ShowCoroutine = null;
                }

                m_ShowCoroutine = StartCoroutine(ShowPopupCoroutine(popupToShow));
            }

        }

        Popup GetNextPopup()
        {
            if ( m_ErrorPopups.Count > 0 )
                return m_ErrorPopups[0];

            if (m_WarningPopups.Count > 0)
                return m_WarningPopups[0];

            if (m_InfoPopups.Count > 0)
                return m_InfoPopups[0];

            return null;
        }

        void AddPopupToLists( Popup popup )
        {
            switch (popup.Priority)
            {
                case PopupPriorities.Error: m_ErrorPopups.Add(popup); break;

                case PopupPriorities.Warning: m_WarningPopups.Add(popup); break;

                case PopupPriorities.Info: m_InfoPopups.Add(popup); break;

                default: m_InfoPopups.Add(popup); break;
            }
        }

        void AddPopupToListsAsFirst(Popup popup)
        {
            switch (popup.Priority)
            {
                case PopupPriorities.Error: m_ErrorPopups.Insert(0, popup); break;

                case PopupPriorities.Warning: m_WarningPopups.Insert(0, popup); break;

                case PopupPriorities.Info: m_InfoPopups.Insert(0, popup); break;

                default: m_InfoPopups.Insert(0, popup); break;
            }
        }

        void RemovePopupFromLists(Popup popup)
        {
            switch (popup.Priority)
            {
                case PopupPriorities.Error: m_ErrorPopups.Remove(popup); break;

                case PopupPriorities.Warning: m_WarningPopups.Remove(popup); break;

                case PopupPriorities.Info: m_InfoPopups.Remove(popup); break;

                default: m_InfoPopups.Remove(popup); break;
            }
        }

        #endregion Private functions

        #region Coroutines

        IEnumerator ShowPopupCoroutine(Popup popup)
        {
            if(popup == null)
                yield break;

            if ( m_CurrentPopup != null )
            {
                if ( m_HideCoroutine != null )
                {
                    StopCoroutine(m_HideCoroutine);
                    m_HideCoroutine = null;
                }
                
                AddPopupToListsAsFirst(m_CurrentPopup);
            }

            m_CurrentPopup = popup;

            m_Displayer.Show( popup, m_DefaultStyle );

            yield return new WaitWhile( () => m_Displayer.ShowCoroutineIsRunning );

            m_ShowCoroutine = null;
        }

        IEnumerator HidePopupCoroutine()
        {
            m_Displayer.Hide();

            yield return new WaitWhile(() => m_Displayer.HideCoroutineIsRunning );

            m_CurrentPopup = null;

            m_HideCoroutine = null;

            ShowNextPopup();
        }
    }

    #endregion Coroutines
}