using Nuro.Processes;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using VRWeb.Developer;
using VRWeb.Managers;

namespace VRWeb.Processes
{
	public class DownloadTextureWebRequestProcess : Process
    {
        // Private variables
        private string m_URI = null;

        private UnityWebRequest m_WebRequest = null;

        private Texture2D m_ResultTexture = null;

        // Getter
        public override float Progress =>
            m_WebRequest == null ? 0.0f : m_WebRequest.downloadProgress + m_WebRequest.uploadProgress;

        public string URI => m_URI;

        public Texture2D ResultTexture => m_ResultTexture;

        #region Public

        // Constructor
        public DownloadTextureWebRequestProcess( string name, string uri, Sprite icon ) : base( name, icon )
        {
            m_URI = HopperRoot.Get<UrlMapper>().MappedUrl( uri );
        }

        // Public function
        public Sprite ResultToSprite()
        {
            return Sprite.Create(
                ResultTexture,
                new Rect( 0.0f, 0.0f, ResultTexture.width, ResultTexture.height ),
                new Vector2( 0.5f, 0.5f ),
                100.0f );
        }

        #endregion

        #region Protected

        protected override IEnumerator ProcessFunc()
        {
            m_WebRequest = UnityWebRequestTexture.GetTexture( m_URI );

            m_Status = Process.ProcessStatus.Running;

            yield return m_WebRequest.SendWebRequest();

            if ( m_WebRequest.result == UnityWebRequest.Result.Success )
            {
                m_Status = Process.ProcessStatus.Succeeded;
                m_ResultTexture = DownloadHandlerTexture.GetContent( m_WebRequest );
                OnSuccess();
            }
            else
            {
                m_Status = Process.ProcessStatus.Failed;
                OnError( m_WebRequest.result );
            }
        }

        protected virtual void OnSuccess()
        {

        }

        protected virtual void OnError( UnityWebRequest.Result errorResult )
        {

        }

        #endregion
    }

}