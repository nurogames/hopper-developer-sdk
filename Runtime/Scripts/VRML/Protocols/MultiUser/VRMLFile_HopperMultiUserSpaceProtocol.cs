using System.Xml;
using UnityEngine.Scripting;

namespace VRWeb.VRML.Protocols
{

    [Preserve]
    public class VRMLFile_HopperMultiUserSpaceProtocol : VRMLFile_Protocol
	{
		// Private variables
		private new const string PROTOCOL_NAME = "HOPPER_MULTI_USER_SPACE";
		private const string VERSION = "1";

		private bool m_IsOptional = false;

		private const string HOST = "host";
		private const string PORT = "port";


		private string m_Host = "";
		private string m_Port = "";

		//Getters
		public override string ProtocolName => PROTOCOL_NAME;
		public override string ProtocolVersion => VERSION;
		public override bool IsOptional => m_IsOptional;

		public string Host => m_Host;
		public string Port => m_Port;

		// Static Constructor
		static VRMLFile_HopperMultiUserSpaceProtocol()
		{
			VRMLFileHelper.RegisterProtocolType<VRMLFile_HopperMultiUserSpaceProtocol>(PROTOCOL_NAME, VERSION);
		}

		// Public functions
		public override void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root)
		{
			XmlElement protocol = VRMLFileHelper.CreateProtocolElement(xmlDoc, root, PROTOCOL_NAME, m_IsOptional);

			XmlElement host = xmlDoc.CreateElement(HOST);
			host.InnerText = m_Host;
			protocol.AppendChild(host);

			XmlElement port = xmlDoc.CreateElement(PORT);
			port.InnerText = m_Port;
			protocol.AppendChild(port);
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

			XmlElement host = VRMLFileHelper.GetFirstElementWithName(protocol, HOST);
			if (protocol == null)
				return false;

			XmlElement port = VRMLFileHelper.GetFirstElementWithName(protocol, PORT);
			if (port == null)
				return false;

			m_Host = host.InnerText;
			m_Port = port.InnerText;

			return true;
		}
	}
}
