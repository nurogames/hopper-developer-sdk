using System.Xml;

namespace VRWeb.VRML
{
	public class VRMLFile_CreatorInfo
    {
		// Private variables
		private const string CREATOR_INFO = "creatorInfo";

		private const string NAME = "name";
		private const string LEGAL_NOTICE = "legalNotice";
		private const string LEGAL_NOTICE_TYPE = "type";
		private const string COPYRIGHT = "copyright";

		private string m_Name = "UNKNOWN";
        private bool m_IsLegalNoticeText = true;
        private string m_LegalNotice = "NO LEGAL NOTICE ATTACHED";
        private string m_Copyright = "NO COPYRIGHT";

        // Getters
        public string Name => m_Name;
        public bool IsLegalNoticeText => m_IsLegalNoticeText;
        public string LegalNotice => m_LegalNotice;
        public string Copyright => m_Copyright;

        // Public functions
        public void WriteToXmlDocument(XmlDocument xmlDoc, XmlElement root)
        {
			XmlElement creatorInfoRoot = xmlDoc.CreateElement(CREATOR_INFO);

			XmlElement name = xmlDoc.CreateElement(Name);
			name.InnerText = m_Name;
			creatorInfoRoot.AppendChild(name);

			XmlElement legalNotice = xmlDoc.CreateElement(LEGAL_NOTICE);
			legalNotice.InnerText = m_LegalNotice;
			XmlAttribute legalNoticeType = xmlDoc.CreateAttribute(LEGAL_NOTICE_TYPE);
			legalNoticeType.InnerText = m_IsLegalNoticeText ? "text" : "link";
			legalNotice.Attributes.Append(legalNoticeType);
			creatorInfoRoot.AppendChild(legalNotice);

			XmlElement copyright = xmlDoc.CreateElement(COPYRIGHT);
			copyright.InnerText = m_Copyright;
			creatorInfoRoot.AppendChild(copyright);

			root.AppendChild(creatorInfoRoot);
		}

        public bool LoadFromXml(XmlElement root)
        {
            if(root == null)
                return false;

			XmlElement creatorInfoRoot = VRMLFileHelper.GetFirstElementWithName(root, CREATOR_INFO);
			if(creatorInfoRoot == null)
				return false;

			XmlElement name = VRMLFileHelper.GetFirstElementWithName(creatorInfoRoot, NAME);
			if(name == null) 
				return false;

			XmlElement legalNotice = VRMLFileHelper.GetFirstElementWithName(creatorInfoRoot, LEGAL_NOTICE);
			if (legalNotice == null)
				return false;

			XmlElement copyright = VRMLFileHelper.GetFirstElementWithName(creatorInfoRoot, COPYRIGHT);
			if (copyright == null)
				return false;

			XmlAttribute legalNoticeType = legalNotice.GetAttributeNode(LEGAL_NOTICE_TYPE);
			if (legalNoticeType != null && legalNoticeType.InnerText == "link")
				m_IsLegalNoticeText = false;

			m_Name = name.InnerText;
			m_LegalNotice = legalNotice.InnerText;
			m_Copyright = copyright.InnerText;

			return true;
        }
    }
}