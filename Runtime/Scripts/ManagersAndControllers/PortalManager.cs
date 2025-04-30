using Nuro.Processes;
using System;
using System.Collections;
using System.Collections.Generic;
using Nuro.VRWeb.NetworkMessages;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Core.Settings;
using VRWeb.Events;
using VRWeb.User;
using VRWeb.VRML;
using VRWeb.VRML.Processes;
using VRWeb.VRML.Protocols;

namespace VRWeb.Managers
{
	public class PortalManager : HopperManagerMonoBehaviour <PortalManager>,IPortalManager
    {
		[SerializeField] SceneLoadedEvent m_OnLoadedEvent;

		// Private variables
		private Dictionary<string, Portal> m_Portals = new Dictionary<string, Portal> ();

		private List<Portal> m_LoadedPortals = new List<Portal>();

		private PersistentParams m_PersistentParams = new PersistentParams ();

		private const string DEFAULT_HOME_URL = "http://experimental.nuromedia.com/HUB.vrml";
        //private const string DEFAULT_HOME_URL = "http://experimental.nuromedia.com/hecof/NTUA/NTUA_ChangingRoomV2.vrml";

        private string m_HomeUrl = null;
		private string m_LastVisitedUrl = null;

        // Events
        public event Action<Portal, SceneLoadedEvent.LoadStatus> onPortalLoaded;

		// Getter
		public List<Portal> LoadedPortals => new List<Portal> (m_LoadedPortals);
		public Portal LastLoadedPortal => m_LoadedPortals.Count > 0 ? m_LoadedPortals[m_LoadedPortals.Count - 1] : null;
		public PersistentParams PersistentParams => m_PersistentParams;
		public string HomeUrl => string.IsNullOrEmpty(m_HomeUrl) ? DEFAULT_HOME_URL : m_HomeUrl;
		public string LastVisitedUrl => m_LastVisitedUrl;
		
        public string CurrentVrmlUrl { get; set; }

        private void Awake()
        {
            RegisterManager();
        }

        private void OnEnable()
        {
            onPortalLoaded += OnPortalLoaded;
        }

        private void OnDisable()
        {
            onPortalLoaded -= OnPortalLoaded;
        }


		// Public functions
        public void GoHome()
        {
			LoadPortal( HomeUrl );
        }

        public string GetParamAsString( string key, string defaultValue = "" )
        {
			return m_PersistentParams.GetParamAsString(key, defaultValue);
        }
		
        public void FetchPortal(string url, Action<Portal> onPortalFetched = null)
		{
			if (string.IsNullOrEmpty(url))
				return;

			if(m_Portals.TryGetValue(url, out Portal portal))
			{
				onPortalFetched?.Invoke(portal);
				return;
			}

			GetVRMLFileWebRequestProcess process = new GetVRMLFileWebRequestProcess(url);
			process.onFinished += (status) =>
			{
				if(process.File != null)
				{
					Portal portal = new Portal(url, process.File);
					m_Portals[url] = portal;
					onPortalFetched?.Invoke(portal);
				}
			};

			process.Start();
		}

		public void LoadPortal(string url)
		{
            if (string.IsNullOrEmpty(url) || url.Length <= 5 )
			{
				HandleFileFetchError(url);
				return;
			}

			url = url.Trim(new char[] {'\r','\n'});

            if ( !url.EndsWith( "vrml" ) && !string.IsNullOrEmpty( CurrentVrmlUrl ) )
            {
                int index = CurrentVrmlUrl.LastIndexOf( "/" );
                string rootUrl = CurrentVrmlUrl.Substring( 0, index );
                url = rootUrl + "/" + url + ".vrml";
            }

            //if (m_Portals.TryGetValue(url, out Portal portal))
            //{
            //	LoadPortal(portal);
            //	return;
            //}

            GetVRMLFileWebRequestProcess process = new GetVRMLFileWebRequestProcess(url);
			process.onFinished += (status) =>
			{
				if (process.File != null)
				{
					Portal portal = new Portal(url, process.File);
					m_Portals[url] = portal;
					LoadPortal(portal);
				}
				else
					HandleFileFetchError(url);
			};

			process.Start();
		}

		public void LoadPortal(Portal portal)
		{
			if (portal == null || !portal.IsValid())
			{
				LoadingOfPortalFailedError(portal);
				return;
			}

			VRMLFile_ErrorProtocol error = portal.GetErrorProtocol();
			if(error != null)
			{
				HandleError(error.Id, error.Message);
				return;
			}

			if (LastLoadedPortal != null && LastLoadedPortal.VRMLFile.RootDomain != portal.VRMLFile.RootDomain)
			{
				m_LoadedPortals.Clear();
				PersistentParams.ClearAllParams();
			}

			PersistentParams.FetchParamsFromUrl(portal.Url);

			LoadPortalProcessV2 process = new LoadPortalProcessV2(portal);
			process.onFinished += (status) =>
			{
				if (status == Process.ProcessStatus.Succeeded)
				{ 
					m_LastVisitedUrl = portal.VRMLFile.RootDomain;
                    CurrentVrmlUrl = portal.Url;
                    m_LoadedPortals.Add(portal);
					onPortalLoaded(portal, SceneLoadedEvent.LoadStatus.finishLoad);
				}
				else
                {
                    onPortalLoaded( portal, SceneLoadedEvent.LoadStatus.cancelLoad );
                    LoadingOfPortalFailedError( portal );
                }
            };

            onPortalLoaded( portal, SceneLoadedEvent.LoadStatus.beginLoad );
            process.Start();
		}

		public void SetHomeUrl(string url)
		{
			if(!string.IsNullOrEmpty(url))
				m_HomeUrl = url;
            else
                m_HomeUrl = DEFAULT_HOME_URL;
        }

		// Private functions
		private void HandleFileFetchError(string url)
		{
			HandleError(404, $"Failed to fetch vrml at url: \"{url}\"");
		}

		private void LoadingOfPortalFailedError(Portal portal)
		{
			HandleError(400, $"Failed to load portal from url: \"{portal.Url}\"");
		}

		private void OnPortalLoaded(Portal portal, 
            SceneLoadedEvent.LoadStatus status=SceneLoadedEvent.LoadStatus.finishLoad)
		{
			Debug.Log("[PortalManager] Portal loaded: " + portal.Url);

			m_OnLoadedEvent?.OnSceneLoaded( status );
		}

		private void HandleError(int code, string message)
		{
			Debug.LogError($"[PortalManager] {code} | {message}");

			SceneHandler.Instance.EnterErrorScene(code, message);
		}
    }
}