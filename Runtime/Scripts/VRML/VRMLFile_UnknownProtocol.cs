using System.Xml;

namespace VRWeb.VRML.Protocols
{
	public class VRMLFile_UnknownProtocol : VRMLFile_Protocol
	{
		// Private variables
		private new const string PROTOCOL_NAME = "UNKNOWN";
		private const string VERSION = "1";

		private string m_ProtocolName = PROTOCOL_NAME;
		private string m_Version = null;
		private bool m_IsOptional = false;

		//Getters
		public override string ProtocolName => m_ProtocolName;
		public override string ProtocolVersion => m_Version;
		public override bool IsOptional => m_IsOptional;

		// Static Constructor
		static VRMLFile_UnknownProtocol()
		{
			VRMLFileHelper.RegisterProtocolType<VRMLFile_ErrorProtocol>(PROTOCOL_NAME, VERSION);
		}

		// Public functions
		public override void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root)
		{
			XmlElement protocol = VRMLFileHelper.CreateProtocolElement(xmlDoc, root, PROTOCOL_NAME, m_IsOptional);
		}

		public override bool LoadFromXml(XmlElement protocol)
		{
			if (protocol == null)
				return false;

			m_ProtocolName = protocol.GetAttribute(VRMLFile_Protocol.PROTOCOL_NAME);

			m_Version = protocol.HasAttribute(PROTOCOL_VERSION) ? protocol.GetAttribute(PROTOCOL_VERSION) : null;

			if(protocol.HasAttribute(PROTOCOL_OPTIONAL))
			{
				string optionalValue = protocol.GetAttribute(PROTOCOL_OPTIONAL);
				bool.TryParse(optionalValue, out bool m_IsOptional);
			}

			return true;
		}

	}
}
