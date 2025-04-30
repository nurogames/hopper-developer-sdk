using System.Xml;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace VRWeb.VRML
{

	public struct VRMLMetaInfos
    {
        public string m_Name;
        public bool m_NameIsVisible;
        public Color m_NameColor;
        public string m_Description;
        public string m_Icon;
        public static VRMLMetaInfos Default => new VRMLMetaInfos()
        {
            m_NameIsVisible = true,
            m_NameColor = Color.white
        };
    }
    
    public static class VRMLMetaInfosHelper
    {
        const string META_INFOS = "MetaInfos";

        const string NAME = "Name";
        const string NAME_VISIBLE = "visibile";
        const string NAME_COLOR = "color";
        const string DESCRIPTION = "Description";
        const string ICON = "Icon";

        public static XmlElement CreateMetaInfosXmlElement(VRMLMetaInfos metaInfos, XmlDocument xmlDoc)
        {
            XmlElement metaInfosRoot = xmlDoc.CreateElement( META_INFOS );
            
            XmlElement name = xmlDoc.CreateElement(NAME);
            name.InnerText = metaInfos.m_Name;
            
            XmlAttribute visible = xmlDoc.CreateAttribute( NAME_VISIBLE );
            visible.InnerText = metaInfos.m_NameIsVisible.ToString();
            name.Attributes.Append( visible );

            XmlAttribute color = xmlDoc.CreateAttribute( NAME_COLOR );
            color.InnerText = $"#{ColorUtility.ToHtmlStringRGB(metaInfos.m_NameColor)}";
            name.Attributes.Append( color );

            metaInfosRoot.AppendChild(name);

            XmlElement description = xmlDoc.CreateElement(DESCRIPTION);
            description.InnerText = metaInfos.m_Description;
            metaInfosRoot.AppendChild(description);

            XmlElement icon = xmlDoc.CreateElement(ICON);
            icon.InnerText = metaInfos.m_Icon;
            metaInfosRoot.AppendChild(icon);

            return metaInfosRoot;
        }

        public static VRMLMetaInfos CreateMetaInfos(XmlElement root)
        {
            VRMLMetaInfos metaInfos = VRMLMetaInfos.Default;

            XmlElement metaInfosRoot = VRMLFileHelper.GetFirstElementWithName(root, META_INFOS);

            if (metaInfosRoot != null)
            {
                XmlElement name = VRMLFileHelper.GetFirstElementWithName(metaInfosRoot, NAME);

                if ( name != null )
                {
                    metaInfos.m_Name = name.InnerText;
                    
                    if (name.HasAttribute(NAME_VISIBLE))
                        metaInfos.m_NameIsVisible = name.GetAttributeNode(NAME_VISIBLE).Value.ToLower() == "true";

                    if ( name.HasAttribute( NAME_COLOR ) )
                    {
                        string colorString = name.GetAttributeNode( NAME_COLOR ).Value;
                        ColorUtility.TryParseHtmlString( colorString, out metaInfos.m_NameColor );
                    }
                }

                XmlElement mesh = VRMLFileHelper.GetFirstElementWithName(metaInfosRoot, DESCRIPTION);
                if (mesh != null)
                    metaInfos.m_Description = mesh.InnerText;

                XmlElement texture = VRMLFileHelper.GetFirstElementWithName(metaInfosRoot, ICON);
                if (texture != null)
                    metaInfos.m_Icon = texture.InnerText.Trim();
            }

            return metaInfos;
        }
    }
}
