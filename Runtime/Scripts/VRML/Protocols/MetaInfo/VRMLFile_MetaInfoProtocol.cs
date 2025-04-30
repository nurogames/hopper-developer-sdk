using System.Xml;
using UnityEngine;
using UnityEngine.Scripting;
using VRWeb.Utils;

namespace VRWeb.VRML.Protocols
{

    [Preserve]
    public class VRMLFile_MetaInfoProtocol : VRMLFile_Protocol
	{
        // Private variables
        private new const string PROTOCOL_NAME = "META_INFO";
		private const string VERSION = "1";

		private bool m_IsOptional = false;

		const string NAME = "name";
		const string DESCRIPTION = "description";
		const string ICON = "icon";

		private string m_Name;
		private string m_Description;
		private string m_Icon;

		// Getters
		public override string ProtocolName => PROTOCOL_NAME;
		public override string ProtocolVersion => VERSION;
		public override bool IsOptional => m_IsOptional;

		public string Name => m_Name;
		public string Description => m_Description;
		public string Icon => m_Icon;

        // Static Constructor
        static VRMLFile_MetaInfoProtocol()
        {
            Debug.Log("RegisterProtocolType(VRMLFile_MetaInfoProtocol)");
            VRMLFileHelper.RegisterProtocolType<VRMLFile_MetaInfoProtocol>(PROTOCOL_NAME, VERSION);
        }

		// Public functions
		public override void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root)
		{
			XmlElement protocol = VRMLFileHelper.CreateProtocolElement(xmlDoc, root, PROTOCOL_NAME, m_IsOptional);

			XmlElement name = xmlDoc.CreateElement(NAME);
			name.InnerText = m_Name;
			protocol.AppendChild(name);

			XmlElement description = xmlDoc.CreateElement(DESCRIPTION);
			description.InnerText = m_Description;
			protocol.AppendChild(description);

			XmlElement icon = xmlDoc.CreateElement(ICON);
			icon.InnerText = m_Icon;
			protocol.AppendChild(icon);
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

			XmlElement name = VRMLFileHelper.GetFirstElementWithName(protocol, NAME);
			if (protocol == null)
				return false;

			m_Name = name.InnerText;

			XmlElement description = VRMLFileHelper.GetFirstElementWithName(protocol, DESCRIPTION);
			if (description != null)
				m_Description = description.InnerText;

			XmlElement icon = VRMLFileHelper.GetFirstElementWithName(protocol, ICON);
			if (icon != null)
				m_Icon = UrlHelper.Trim(icon.InnerText);

			return true;
		}

		public VRMLMetaInfos GetMetaInfos()
		{
			return new VRMLMetaInfos() { m_Name = m_Name, m_Description = m_Description, m_Icon = m_Icon };
		}
    }
}
