using Nuro.Processes;
using System.Xml;

namespace VRWeb.VRML
{
	public abstract class VRMLFile_Protocol

    {
        public const string PROTOCOL = "protocol";
		public const string PROTOCOL_NAME = "name";
		public const string PROTOCOL_VERSION = "version";
		public const string PROTOCOL_OPTIONAL = "optional";

		public abstract string ProtocolName { get; }
        public abstract string ProtocolVersion { get; }
		public abstract bool IsOptional { get; }
        public string RootDomain { get; set; }
        public uint VersionAttribute { get; set; }

        public abstract void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root);

        public abstract bool LoadFromXml(XmlElement root);

        public virtual Process CreateLoadProcess(Portal portal) => null;
    }
}