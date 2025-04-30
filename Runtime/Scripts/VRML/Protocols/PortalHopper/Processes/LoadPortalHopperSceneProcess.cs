using Nuro.Processes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Creator;
using VRWeb.Managers;
using VRWeb.Rig;
using VRWeb.User;

namespace VRWeb.VRML.Protocols.Processes
{
	public class LoadPortalHopperSceneProcess : Process
    {
        // Private variables
        private string m_SceneAssetBundlePath = null;
        private string m_ScenePath = null;
		private bool m_IsMain = true;
        private VRMLFile_PortalHopperProtocol m_Protocol;
		private string m_VRMLUrl;

        // Constructor
        public LoadPortalHopperSceneProcess( 
			string vrmlUrl,
            VRMLFile_PortalHopperProtocol protocol, 
            string sceneAssetBundlePath, 
            string scenePath, bool isMain) : base($"Load scene: {scenePath}") 
        {
            m_SceneAssetBundlePath = sceneAssetBundlePath;
            m_ScenePath = scenePath;
			m_IsMain = isMain;
            m_Protocol = protocol;
            m_VRMLUrl = vrmlUrl;
        }

		protected override IEnumerator ProcessFunc()
		{
            AssetBundleManager assetBundleManager = HopperRoot.Get<AssetBundleManager>();

            AssetBundleResource sceneAssetBundle = assetBundleManager.TryGetLoadedAssetBundleResource(m_SceneAssetBundlePath);
            if (sceneAssetBundle == null)
            {
                m_Status = ProcessStatus.Failed;
			    yield break;
            }

			AssetBundle assetBundle = sceneAssetBundle.m_AssetBundle;
			if (assetBundle == null)
			{
				m_Status = ProcessStatus.Failed;
				yield break;
			}

			string scenePath = GetScenePathToLoad(assetBundle, m_ScenePath);
			if (string.IsNullOrEmpty(scenePath))
			{
				m_Status = ProcessStatus.Failed;
				yield break;
			}

			AsyncOperation loadSceneAsyncOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

			while (!loadSceneAsyncOperation.isDone)
			{
				ChangeProgress(loadSceneAsyncOperation.progress - 0.1f);
				yield return null;
			}

			if (m_IsMain)
			{
				UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByPath(scenePath);

				yield return new WaitUntil(() => scene.isLoaded);
                
                SceneManager.SetActiveScene(scene);
                HopperRoot.Get<PortalManager>().CurrentVrmlUrl = m_VRMLUrl;

                PortalTarget portalTarget = GameObject.FindAnyObjectByType<PortalTarget>();
				portalTarget?.EnterWorld( m_VRMLUrl );

				UserRig.Instance?.LoadAvatar();
			}
		}

		public string GetScenePathToLoad(AssetBundle bundle, string scenePath)
		{
			string[] scenePaths = bundle.GetAllScenePaths();

			if (scenePaths == null || scenePaths.Length == 0)
			{
				return null;
			}

			return scenePaths.Contains(scenePath) ? scenePath : scenePaths[0];
		}
	}
}