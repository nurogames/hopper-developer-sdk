using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRWeb.Developer;
using VRWeb.Processes;
using Port = VRWeb.VRML.Portal;

namespace VRWeb.Managers
{
	public class AssetBundleManager : HopperManagerMonoBehaviour<AssetBundleManager>
    {
        private Dictionary<string, AssetBundleResource> m_LoadedAssetBundles = new Dictionary<string, AssetBundleResource>();
        private List<Hash128> m_CachedBundleVersionList = new List<Hash128>();

        #region Unity Event Functions

        void Awake()
        {
            RegisterManager();
        }

        private void OnDisable()
        {
            foreach (var kvp in m_LoadedAssetBundles)
            {
                if (kvp.Value.m_AssetBundle != null)
                {
                    kvp.Value.m_AssetBundle.UnloadAsync(true);
                }
            }
        }

        #endregion

        #region Public

        public DownloadAndCacheAssetBundleProcess CreateDownloadAndCacheAssetBundleProcess(string uri, uint version) =>
            new DownloadAndCacheAssetBundleProcess(uri, m_LoadedAssetBundles, version);


        public AssetBundleResource GetLoadedAssetBundleResourceByPath(string path)
        {
            return m_LoadedAssetBundles.TryGetValue(path, out AssetBundleResource bundle) ? bundle : null;
        }

        public AssetBundleResource TryGetLoadedAssetBundleResource(string path)
        {
            AssetBundleResource bundle = GetLoadedAssetBundleResourceByPath(path);

            if (bundle == null)
            {
                string mappedPath = HopperRoot.Get<UrlMapper>().MappedUrl(path);
                bundle = GetLoadedAssetBundleResourceByPath(mappedPath);
            }

            return bundle;
        }

        public bool IsAssetBundleLoaded(string bundleName)
        {
            return m_LoadedAssetBundles.ContainsKey(bundleName);
        }

        public bool IsAssetBundleCached(string bundlePath)
        {
            bundlePath = HopperRoot.Get<UrlMapper>().MappedUrl(bundlePath);

            m_CachedBundleVersionList.Clear();

            string[] bundleNames = bundlePath.Split(new char[] { '/', '\\' });
            string bundleName = bundleNames.Last();
            Caching.GetCachedVersions(bundlePath, m_CachedBundleVersionList);

            return m_CachedBundleVersionList.Count > 0;
        }

        public bool AreAllAssetBundlesCached(Port portal)
        {
            return true;
            /*
            List < string > paths = new List < string >() { HopperRoot.Get<UrlMapper>().MappedUrl(portal.m_SceneContainerPath) };
            if( portal.m_PreloadContainerPaths != null )
                paths.AddRange( portal.m_PreloadContainerPaths );

            return AreAllAssetBundlesCached(paths);*/

        }

        public bool AreAllAssetBundlesCached(List<string> paths)
        {
            foreach (string path in paths)
            {
                if (!IsAssetBundleCached(HopperRoot.Get<UrlMapper>().MappedUrl(path)))
                    return false;
            }

            return true;
        }

        public void RequestObjectFromAssetBundle<T>(string path, string objectName, uint version, Action<T> callback) where T : UnityEngine.Object
        {
            string mappedPath = HopperRoot.Get<UrlMapper>().MappedUrl(path);

            if (m_LoadedAssetBundles.ContainsKey(mappedPath))
            {
                T returnObject = default(T);

                returnObject = m_LoadedAssetBundles[mappedPath].m_AssetBundle.LoadAsset(objectName) as T;
                callback?.Invoke(returnObject);

                return;
            }

            DownloadAndCacheAssetBundleProcess loadAssetBundleProcess = CreateDownloadAndCacheAssetBundleProcess(mappedPath, version);
            loadAssetBundleProcess.onFinished += (status) =>
            {
                T returnObject = default(T);

                if (status == Nuro.Processes.Process.ProcessStatus.Succeeded)
                {
                    returnObject = loadAssetBundleProcess.Bundle.LoadAsset(objectName) as T;
                    callback?.Invoke(returnObject);
                }
                else
                {
                    Debug.LogError($"LoadAsset {objectName} from {mappedPath} failed.");
                }

            };

            loadAssetBundleProcess.Start();
        }

        #endregion
    }

    public class AssetBundleResource
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferenceCount;
    }
}