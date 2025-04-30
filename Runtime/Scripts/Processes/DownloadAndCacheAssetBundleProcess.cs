using Nuro.Processes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VRWeb.Managers;
using VRWeb.UI;

namespace VRWeb.Processes
{
	public class DownloadAndCacheAssetBundleProcess : Process
    {
        private string m_Uri;
        private Dictionary < string, AssetBundleResource > m_BundleDictionary;
        private AssetBundle m_Bundle = null;
        private uint m_Version;

        public AssetBundle Bundle => m_Bundle;

        public DownloadAndCacheAssetBundleProcess(  
            string uri, 
            Dictionary < string, AssetBundleResource > dictionary,
            uint version = 1) : 
            base("Download And Cache Asset Bundle", IconProvider.Instance.DownloadProcess)
        {
            m_Uri = uri;
            m_BundleDictionary = dictionary;
            m_Version = version;
        }

        private bool m_ProcessFuncIsLocked = false;

        protected override IEnumerator ProcessFunc()
        {
            yield return new WaitWhile( () => m_ProcessFuncIsLocked );

            m_ProcessFuncIsLocked = true;

            yield return new WaitUntil( () => Caching.ready );

            if ( Caching.IsVersionCached( m_Uri, new Hash128()) )
            {
                Debug.Log( $"Found {m_Uri} in Unity's internal cache");
            }

            if ( m_BundleDictionary.ContainsKey(m_Uri) )
            {
                Debug.Log( $"skipped loading of {m_Uri} because it is already cached." );

                m_Status = ProcessStatus.Succeeded;
                m_Bundle = m_BundleDictionary[m_Uri].m_AssetBundle;

                m_ProcessFuncIsLocked = false;
                yield break;
            }
            //Download the bundle
            //Hash128 hash = manifest.GetAssetBundleHash( m_BundleName );
            //Debug.Log( $"loading {m_Uri}" );
            
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle( m_Uri, m_Version, 0 );

            request.SendWebRequest();

            yield return new WaitUntil(() => request.isDone );

            UnityWebRequest rrrr = request;

            if ( rrrr.result == UnityWebRequest.Result.Success )
            {
                m_Bundle = DownloadHandlerAssetBundle.GetContent( request );

                if ( !m_BundleDictionary.ContainsKey( m_Uri ) )
                {
                    m_BundleDictionary.Add(
                        m_Uri,
                        new AssetBundleResource() { m_AssetBundle = m_Bundle, m_ReferenceCount = 1 } );
                }
            }
            else
            {
                Debug.LogError( $"failed to load {m_Uri}" );
                m_Status = ProcessStatus.Failed;
            }

            m_ProcessFuncIsLocked = false;
        }
    }
}