using System.Collections.Generic;
using Nuro.Processes;
using System.Xml;
using UnityEngine;
using UnityEngine.Scripting;
using VRWeb.Utils;
using VRWeb.VRML.Protocols.Processes;

namespace VRWeb.VRML.Protocols
{

    [Preserve]
    public class VRMLFile_PortalHopperProtocol : VRMLFile_Protocol
	{
		// Enums
		public enum LoadModes : byte
		{
			Add,
			AddAsMain,
			Replace
		}

		// Private variables
		private new const string PROTOCOL_NAME = "PORTAL_HOPPER_PROTOCOL";
		private const string VERSION_DEFAULT_VALUE = "1";

		private const string ASSET_BUNDLE_PATH = "assetBundlePath";
		private const string SCENE_PATH = "scenePath";
        private const string PRELOAD_PATH = "preloadPath";
		private const string DLL_KEYWORD = "dll";
        private const string LOAD_MODE = "loadMode";
		private const string VERSION_ATTRIBUTE = "version";

        private bool m_IsOptional = false;

		private string m_AssetBundlePath = "";
		private string m_ScenePath = "";
        private List<string> m_DllPaths;

        private LoadModes m_LoadMode = LoadModes.Replace;

		//Getters
		public override string ProtocolName => PROTOCOL_NAME;
		public override string ProtocolVersion => VERSION_DEFAULT_VALUE;
		public override bool IsOptional => m_IsOptional;

		public string AssetBundlePath => m_AssetBundlePath;
        public string ScenePath => m_ScenePath;
		public List<string> DllPaths => m_DllPaths;
		public LoadModes LoadMode => m_LoadMode;
		public bool IsMain => m_LoadMode != LoadModes.Add;

        // Static Constructor
		static VRMLFile_PortalHopperProtocol()
        {
            Debug.Log( "RegisterProtocolType(VRMLFile_PortalHopperProtocol)" );
			VRMLFileHelper.RegisterProtocolType<VRMLFile_PortalHopperProtocol>(PROTOCOL_NAME, VERSION_DEFAULT_VALUE);
		}

		// Public functions
		public override Process CreateLoadProcess(Portal portal)
		{
			return new LoadPortalHopperProtocolProcess(portal, this);
		}

		public override void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root)
		{
			XmlElement protocol = VRMLFileHelper.CreateProtocolElement(xmlDoc, root, PROTOCOL_NAME, m_IsOptional);

			XmlElement assetBundlePath = xmlDoc.CreateElement(ASSET_BUNDLE_PATH);
			assetBundlePath.InnerText = m_AssetBundlePath;
			protocol.AppendChild(assetBundlePath);

			XmlElement scenePath = xmlDoc.CreateElement(SCENE_PATH);
			scenePath.InnerText = m_ScenePath;
			protocol.AppendChild(scenePath);

			XmlElement loadMode = xmlDoc.CreateElement(LOAD_MODE);
			loadMode.InnerText = m_LoadMode.ToString();
			protocol.AppendChild(loadMode);
		}

		public override bool LoadFromXml(XmlElement protocol)
		{
			if (protocol == null)
				return false;

			if (protocol.HasAttribute(PROTOCOL_OPTIONAL))
			{
				string optionalValue = protocol.GetAttribute(PROTOCOL_OPTIONAL);
				bool.TryParse(optionalValue, out bool m_IsOptional);
			}

			XmlElement assetBundlePath = VRMLFileHelper.GetFirstElementWithName(protocol, ASSET_BUNDLE_PATH);
			if (protocol == null)
				return false;

            if ( assetBundlePath.HasAttribute(VERSION_ATTRIBUTE ) )
            {
                string attribute = assetBundlePath.GetAttribute( VERSION_ATTRIBUTE ).Trim();

                if (uint.TryParse(attribute, out uint version))
                    VersionAttribute = version;
            }

			XmlElement scenePath = VRMLFileHelper.GetFirstElementWithName(protocol, SCENE_PATH);
			if (scenePath == null)
				return false;

			m_AssetBundlePath = UrlHelper.Trim(assetBundlePath.InnerText);
			m_ScenePath = UrlHelper.Trim(scenePath.InnerText);

			XmlElement loadMode = VRMLFileHelper.GetFirstElementWithName(protocol, LOAD_MODE);
			if (loadMode != null)
			{
				if (!System.Enum.TryParse(loadMode.InnerText, out m_LoadMode))
					m_LoadMode = LoadModes.Replace;
			}

			XmlElement[] dllPaths = VRMLFileHelper.GetElementsWithTagName( 
                protocol, 
                PRELOAD_PATH,
                DLL_KEYWORD );

            if ( dllPaths != null && dllPaths.Length > 0 )
            {
                foreach ( XmlElement dllPath in dllPaths )
                {
                    m_DllPaths.Add( dllPath.InnerText );
                }
            }

            return true;
		}

		public string GetPatchedPath(string rootDomain)
		{
            if ( rootDomain.ToLower().EndsWith( ".vrml" ) )
            {
				int i = rootDomain.LastIndexOf('/');
				if (i == -1)
					i = rootDomain.LastIndexOf('\\');
                if (i != -1)
                    rootDomain = rootDomain.Substring(0, i );
            }
            return AssetBundlePath.Replace("%ROOT%", rootDomain);
		}
	}
}
