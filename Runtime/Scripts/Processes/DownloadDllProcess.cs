using Nuro.Processes;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VRWeb.Developer;
using VRWeb.Managers;
using VRWeb.UI;

namespace VRWeb.Processes
{
	public class DownloadDllProcess : Process
    {
        private string dllUrl;

        public DownloadDllProcess( string dllUrlFromVRML )
            : base("Download DLL: " + dllUrlFromVRML, IconProvider.Instance.DownloadProcess)
        {
            dllUrl = HopperRoot.Get<UrlMapper>().MappedUrl( dllUrlFromVRML );
        }

        protected override IEnumerator ProcessFunc()
        {
            UnityWebRequest request = new UnityWebRequest( dllUrl );
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if ( request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning( request.error );
                yield break;
            }

            string dllFilePath = HopperRoot.Get<DllCache>().DllUrlToLocalFilename( dllUrl, true );
            File.WriteAllBytes( dllFilePath, request.downloadHandler.data );
            HopperRoot.Get<DllCache>().RegisterDll( dllUrl );
            HopperRoot.Get<DllCache>().LoadDll( dllUrl );
        }
    }
}