using VRWeb.VRML.Protocols;

namespace VRWeb.VRML
{
	public class Portal
    {
        // Private variables
        private string m_Url = null;
        private VRMLFile m_VRMLFile = null;

        // Getter
        public string Url => m_Url;
        public VRMLFile VRMLFile => m_VRMLFile;

        // Constructor
        public Portal() { }

        public Portal(string url, VRMLFile vrmlFile)
        {
            m_Url = url;
            m_VRMLFile = vrmlFile;
        }

        // Public functions
        public bool IsValid()
        {
            if(m_VRMLFile == null)
                return false;

            if (m_VRMLFile.Protocols == null || m_VRMLFile.Protocols.Count == 0)
                return false;

            foreach(VRMLFile_Protocol protocol in m_VRMLFile.Protocols)
            {
                VRMLFile_UnknownProtocol unknownProtocol = protocol as VRMLFile_UnknownProtocol;

				if (unknownProtocol != null && !unknownProtocol.IsOptional)
                    return false;
            }

            return true;
        }

        public VRMLFile_ErrorProtocol GetErrorProtocol()
        {
			foreach (VRMLFile_Protocol protocol in m_VRMLFile.Protocols)
			{
				VRMLFile_ErrorProtocol errorProtocol = protocol as VRMLFile_ErrorProtocol;

				if (errorProtocol != null && !errorProtocol.IsOptional)
                    return errorProtocol;
			}

			return null;
        }

        public VRMLFile_MetaInfoProtocol GetMetaData()
        {
            VRMLFile_MetaInfoProtocol protocol = null;

            m_VRMLFile.GetPrototocolByType<VRMLFile_MetaInfoProtocol>(out protocol);

            return protocol;
        }

        //public VRMLFile_AgeRestrictionProtocol GetAgeRestrictionProtocol()
        //{
        //    VRMLFile_AgeRestrictionProtocol protocol = null;
        //
        //    m_VRMLFile.GetPrototocolByType < VRMLFile_AgeRestrictionProtocol >( out protocol );
        //
        //    return protocol;
        //}
        //
        //public VRMLFile_PortalHopperProtocol GetPortalHopperProtocol()
        //{
        //    VRMLFile_PortalHopperProtocol protocol = null;
        //
        //    m_VRMLFile.GetPrototocolByType <VRMLFile_PortalHopperProtocol>( out protocol );
        //
        //    return protocol;
        //}
    }
}