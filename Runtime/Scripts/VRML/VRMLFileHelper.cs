using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace VRWeb.VRML
{
	public static class VRMLFileHelper
    {
        private const string VRML = "VRML";
        private const string VERSION = "version";

        private static Dictionary<string, Dictionary<string, Type>> m_ProtocolTypes = new Dictionary<string, Dictionary<string, Type>>();

        // Static constructor
        static VRMLFileHelper()
		{
			Type baseType = typeof(VRMLFile_Protocol);
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
                if (assembly.FullName.Contains("Timeline"))
                    continue;

                Type[] types = assembly.GetTypes().Where(
                    myType => myType.IsClass && 
                              !myType.IsAbstract && 
                              !myType.IsAbstract && 
                              myType.IsSubclassOf(baseType)).ToArray();

				foreach (Type type in types)
				{
                    //Debug.Log("scanning protocol assembly: " + type.FullName);
					System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
				}
			}
        }

        // Public functions
        public static XmlElement GetFirstElementWithName(XmlElement root, string tag)
        {
            XmlNodeList list = root.GetElementsByTagName( tag );
            return list != null && list.Count > 0 ? list[0] as XmlElement : null;
        }

        public static XmlElement[] GetElementsWithTagName( XmlElement root, string parentTag, string tag )
        {
            List<XmlElement> returnElements = new List < XmlElement >();

            XmlNodeList parentList = root.GetElementsByTagName( parentTag );
            if (parentList.Count == 0)
                return null;

            XmlNodeList childList = (parentList[0] as XmlElement).GetElementsByTagName( tag );
            foreach ( XmlElement e2 in childList )
            {
                if (!string.IsNullOrEmpty(e2.InnerText))
                    returnElements.Add( e2 );
            }

            return returnElements.ToArray();
        }

        public static XmlElement CreateProtocolElement(XmlDocument doc, XmlElement root, string protocolName, bool isOptional = false)
        {
            XmlElement protocolElement = doc.CreateElement(VRMLFile_Protocol.PROTOCOL );
            XmlAttribute protocolNameElement = doc.CreateAttribute(VRMLFile_Protocol.PROTOCOL_NAME);
            protocolNameElement.Value = protocolName;
            protocolElement.AppendChild(protocolNameElement);

            if(isOptional)
            {
                XmlAttribute optional = doc.CreateAttribute(VRMLFile_Protocol.PROTOCOL_OPTIONAL);
                optional.Value = isOptional.ToString();
                protocolElement.AppendChild(optional);
            }

            root.AppendChild(protocolElement);

            return protocolElement;
        }

        public static Type GetProtocolTypeByName( string name, string version = null )
        {
            if(m_ProtocolTypes.TryGetValue( name, out Dictionary<string, Type> dic) )
			{
				if (string.IsNullOrEmpty(version))
                    return dic.Values.LastOrDefault();

                if(dic.TryGetValue( version, out Type type) )
                    return type;
			}

			return null;
        }

        public static string GetNameByProtocolType(Type type)
        { 
            foreach(KeyValuePair<string, Dictionary<string, Type>> protocolKVP in m_ProtocolTypes)
            {
                foreach (KeyValuePair<string, Type> typeKVP in protocolKVP.Value)
                {
                    if (typeKVP.Value == type)
                        return protocolKVP.Key;
                }
            }

            return null;
        }

        public static void RegisterProtocolType<T>(string protocolName, string version) where T : VRMLFile_Protocol
		{
            if(string.IsNullOrEmpty(protocolName) || string.IsNullOrEmpty(version))
            {
                Debug.LogError("Failed to register protocol type: name: " +  protocolName + " | version: " + version);
                return;
            }

			if (m_ProtocolTypes.ContainsKey(protocolName))
            {
                Dictionary<string, Type> versions = m_ProtocolTypes[protocolName];
				if (!versions.ContainsKey(version))
					versions.Add(version, typeof(T));
				else
					Debug.LogError("Duplicate protocol ignored: protocol: " + protocolName + " | version: " + version);
			}
            else
            {
                Dictionary<string, Type> versions = new Dictionary<string, Type>();
				versions.Add(version, typeof(T));
                m_ProtocolTypes.Add(protocolName, versions);
			}
        }
    }
}