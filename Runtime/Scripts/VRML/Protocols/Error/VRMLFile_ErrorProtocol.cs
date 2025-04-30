using System.Xml;
using UnityEngine.Scripting;

namespace VRWeb.VRML.Protocols
{

    [Preserve]
    public class VRMLFile_ErrorProtocol : VRMLFile_Protocol
	{
		// Private variables
		private new const string PROTOCOL_NAME = "ERROR";
		private const string VERSION = "1";

		private bool m_IsOptional = false;

		const string ID = "id";
		const string MESSAGE = "message";

		private int m_Id;
		private string m_Message;

		//Getters
		public override string ProtocolName => PROTOCOL_NAME;
		public override string ProtocolVersion => VERSION;
		public override bool IsOptional => m_IsOptional;

		public int Id => m_Id;
		public string Message => m_Message;

		// Static Constructor
		static VRMLFile_ErrorProtocol()
		{
			VRMLFileHelper.RegisterProtocolType<VRMLFile_ErrorProtocol>(PROTOCOL_NAME, VERSION);
		}

		// Public functions
		public override void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root)
		{
			XmlElement protocol = VRMLFileHelper.CreateProtocolElement(xmlDoc, root, PROTOCOL_NAME, m_IsOptional);

			XmlElement id = xmlDoc.CreateElement(ID);
			id.InnerText = m_Id.ToString();
			protocol.AppendChild(id);

			XmlElement message = xmlDoc.CreateElement(Message);
			message.InnerText = m_Message;
			protocol.AppendChild(message);
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

			XmlElement id = VRMLFileHelper.GetFirstElementWithName(protocol, ID);
			if (id == null)
				return false;

			XmlElement message = VRMLFileHelper.GetFirstElementWithName(protocol, MESSAGE);
			if (message == null)
				return false;
			
			if(!int.TryParse(id.InnerText, out m_Id))
				m_Id = -1;

			m_Message = message.InnerText;


			return true;
		}

	}
}
