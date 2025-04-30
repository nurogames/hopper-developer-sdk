using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;
using VRWeb.VRML.Protocols;

namespace VRWeb.VRML
{
	public class VRMLFile
    {
        // Private variables
        private string m_Version = null;
        private string m_RootDomain = null;
        private VRMLFile_CreatorInfo m_CreatorInfo = new VRMLFile_CreatorInfo();
        private List<Type> m_ProtocolTypes = new List<Type>();
        private List<VRMLFile_Protocol> m_Protocols = new List<VRMLFile_Protocol>();

        private const string VRML = "vrml";
        private const string VERSION = "version";
        private const string ROOT_DOMAIN = "rootDomain";

        // Getter
        public string FileVersion => m_Version;
        public string RootDomain => m_RootDomain;
        public VRMLFile_CreatorInfo CreatorInfo => m_CreatorInfo;
        public List<VRMLFile_Protocol> Protocols => new List<VRMLFile_Protocol>(m_Protocols);

        // Public functions
        public override string ToString()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement(VRML);
            XmlAttribute version = xmlDoc.CreateAttribute(VERSION);
            version.InnerText = "2.0";
            root.Attributes.Append(version);

            m_CreatorInfo.WriteToXmlDocument(xmlDoc, root);
            foreach(VRMLFile_Protocol protocol in m_Protocols)
            {
                protocol.WriteToXmlDocument(xmlDoc, root);
            }

            xmlDoc.AppendChild(root);
            string vrmlFileString = "";
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    xmlDoc.WriteTo(tx);
                    vrmlFileString = sw.ToString();
                }
            }

            CultureInfo.CurrentCulture = culture;

            return vrmlFileString;
        }

        public bool LoadFromString(string fileText)
		{
			if (string.IsNullOrEmpty(fileText))
                return false;

            CultureInfo culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml( fileText );
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
                CultureInfo.CurrentCulture = culture;

                return false;
            }

            XmlElement root = xmlDoc.DocumentElement;

            if (root.HasAttribute(VERSION))
                m_Version = root.GetAttributeNode(VERSION).Value;

            XmlElement rootDomainRoot = VRMLFileHelper.GetFirstElementWithName( root, ROOT_DOMAIN );
            if ( rootDomainRoot == null )
                return false;
            m_RootDomain = rootDomainRoot.InnerText;

            if (!m_CreatorInfo.LoadFromXml(root))
            {
                CultureInfo.CurrentCulture = culture;
                return false;
            }

            if (!LoadProtocolsFromXml(root))
            {
                CultureInfo.CurrentCulture = culture;
                return false;
            }

            CultureInfo.CurrentCulture = culture;

			return true;
        }

        public bool GetProtocolByName(string name, out VRMLFile_Protocol protocol)
        {
            Type type = VRMLFileHelper.GetProtocolTypeByName(name);
            int index = m_ProtocolTypes.FindIndex(x => x == type);
            if (index == -1)
            {
                protocol = null;
                return false;
            }
            protocol = m_Protocols[index];
            return true;
        }

        public bool GetPrototocolByType<T>(out T protocol) where T : VRMLFile_Protocol
		{
            Type type = typeof(T);

			VRMLFile_Protocol protocolBase;
            int index = m_ProtocolTypes.FindIndex(x => x == type);
            if (index == -1)
            {
                protocol = null;
                return false;
            }

            protocolBase = m_Protocols[index];
            protocol = protocolBase as T;
            return protocol != null;
        }

        public List<Type> GetProtocolTypeList()
        {
            return new List<Type>(m_ProtocolTypes);
        }

        public List<string> GetProtocolList()
        {
            List<string> list = new List<string>();
            foreach(Type type in m_ProtocolTypes)
            {
                list.Add(VRMLFileHelper.GetNameByProtocolType(type));
            }
            return list;
        }

        // Private functions
        private bool LoadProtocolsFromXml(XmlElement root)
        {
            if(root == null) 
                return false;

            XmlNodeList protocols = root.GetElementsByTagName(VRMLFile_Protocol.PROTOCOL);
            foreach (XmlElement protocolXml in protocols)
            {
                if (!protocolXml.HasAttribute(VRMLFile_Protocol.PROTOCOL_NAME))
                    continue;

                if (!LoadProtocol(protocolXml))
                    return false;
            }

            return true;
        }

        private bool LoadProtocol(XmlElement protocolXML)
        {
            string protocolName = protocolXML.GetAttribute(VRMLFile_Protocol.PROTOCOL_NAME);

			string version = protocolXML.HasAttribute(VRMLFile_Protocol.PROTOCOL_VERSION) ? protocolXML.GetAttribute(VRMLFile_Protocol.PROTOCOL_VERSION) : null;

            Type type = VRMLFileHelper.GetProtocolTypeByName(protocolName, version);

            if (type == null)
            {
                type = typeof(VRMLFile_UnknownProtocol);
                UnityEngine.Debug.LogError("[VRMLFile] Couldn't find protocol: " + protocolName + " and version " + version);
            }

			VRMLFile_Protocol protocol = Activator.CreateInstance(type) as VRMLFile_Protocol;
            protocol.RootDomain = RootDomain;

			bool protocolLoadResult = protocol != null ? protocol.LoadFromXml(protocolXML) : false;

            if (!protocolLoadResult)
            {
                type = typeof(VRMLFile_UnknownProtocol);
				protocol = Activator.CreateInstance(type) as VRMLFile_Protocol;
				protocolLoadResult = protocol != null ? protocol.LoadFromXml(protocolXML) : false;
            }

            if ( protocolLoadResult )
                UnityEngine.Debug.Log(
                    "[VRMLFile] Found and loaded protocol: " + type.FullName );

            m_ProtocolTypes.Add(type);
            m_Protocols.Add(protocol);

            return true;
        }
    }
}
