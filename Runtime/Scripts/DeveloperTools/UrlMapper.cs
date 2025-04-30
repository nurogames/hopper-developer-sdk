using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRWeb.Managers;

namespace VRWeb.Developer
{
	public class UrlMapper : HopperManagerMonoBehaviour <UrlMapper>
    {
        [SerializeField]
        private string m_MapToURL;

#if UNITY_EDITOR
        [SerializeField]
        [Tooltip("activate deactivate mapping of URLs in Portals")]
        private bool m_ActivateMapping = false;
#endif

        private void Awake()
        {
            RegisterManager();
        }


#if UNITY_EDITOR
        private IEnumerator Start()
        {
            yield return new WaitUntil( () => HopperRoot.Get<CommandlineParser>() != null );

            CommandlineParser parser = HopperRoot.Get<CommandlineParser>();
            Dictionary < string, string > parameters = parser.Parameters;
            string returnParam = parser.GetArg( parameters, "-map" );

            if ( returnParam != null )
            {
                m_ActivateMapping = true;

                if ( returnParam != "" )
                {
                    m_MapToURL = returnParam;
                }
                else
                {
                    m_MapToURL = "http://localhost";
                }
            }
        }
#endif

        public string MappedUrl(string url)
        {
#if UNITY_EDITOR
                if ( !m_ActivateMapping )
                {
                    return url;
                }

                Uri uri = new Uri( url );
                if ( url.StartsWith( m_MapToURL ) )
                    return url;

                string domain = uri.Host;
                string absolutePath = uri.AbsolutePath;
                string result = m_MapToURL + $"/{domain}{absolutePath}";
                //Debug.Log( $"Mapping \"{url}\" to \"{result}\"" );
                return result;
#else
                return url;
#endif
        }

        string RebuildLink(string[] links)
        {
            StringBuilder result = new();

            foreach (string link in links)
            {
                result.Append(link == "" ? "/" : link);
            }

            return result.ToString();
        }
    }
}