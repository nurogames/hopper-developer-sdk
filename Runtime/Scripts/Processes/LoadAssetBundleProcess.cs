using Nuro.Processes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VRWeb.Managers;
using VRWeb.UI;

namespace VRWeb.Processes
{

	public class LoadAssetBundleProcess : Process
    {
        private string m_Path;
        private AssetBundle m_Bundle;

        public AssetBundle Bundle => m_Bundle;


        private Dictionary < string, AssetBundleResource > m_LoadedAssetBundles = null;

        public LoadAssetBundleProcess( string path, Dictionary<string, AssetBundleResource> loadedAssetBundles ) : 
            base("Load Asset Bundle: " + path, IconProvider.Instance ? IconProvider.Instance.DownloadProcess : null )
        {
            m_Path = path;
            m_LoadedAssetBundles = loadedAssetBundles;
        }

        protected override IEnumerator ProcessFunc()
        {
            if ( string.IsNullOrEmpty( m_Path ) || m_LoadedAssetBundles == null )
            {
                m_Status = ProcessStatus.Failed;

                yield break;
            }

            if ( m_LoadedAssetBundles.ContainsKey( m_Path ) )
            {
                m_LoadedAssetBundles[m_Path].m_ReferenceCount++;
                AssetBundleResource res = m_LoadedAssetBundles[m_Path];
                m_Bundle = res.m_AssetBundle;
                m_Status = ProcessStatus.Succeeded;

                yield break;
            }

            UnityWebRequest unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle( m_Path );
            UnityWebRequestAsyncOperation operation = unityWebRequest.SendWebRequest();

            ChangeProgress( 0.05f );

            while ( !operation.isDone )
            {
                ChangeProgress( operation.progress - 0.1f );

                yield return null;
            }

            if ( unityWebRequest.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( unityWebRequest.error );
                m_Status = ProcessStatus.Failed;

                yield break;
            }

            try
            {
                m_Bundle = DownloadHandlerAssetBundle.GetContent( operation.webRequest );

                m_LoadedAssetBundles.Add(
                    m_Path,
                    new AssetBundleResource() { m_AssetBundle = m_Bundle, m_ReferenceCount = 1 } );
            }
            catch
            {
            }
        }
    }
}
