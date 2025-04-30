using System;
using System.Collections.Generic;
using System.Text;

namespace VRWeb.Utils
{
	[System.Serializable]
	public class PortalParams
	{
		public string key;
		public string value;
	}

	public static class UrlHelper
    {
        public static string CleanUrl(string url)
        {
            if ( string.IsNullOrEmpty( url ) )
                return url;

            url = url.Trim( '"' );
			if ( url.StartsWith("http"))
            {
                Uri uri = new Uri( url );
                string path = uri.GetLeftPart( UriPartial.Path );
                return path.TrimEnd('/');
            }
			// a filename?
            string[] partialPath = url.Split( '?' );
            return partialPath[0];
        }

		public static string Trim( string url )
		{
			return url.Trim(new char[] { '\n', '\r', ' ', '\t' });
		}

		public static Dictionary<string, string> GetParamsFromUrl(string url)
		{
			Dictionary<string, string> parameter = new Dictionary<string, string>();

			int questionMarkIndex = url.IndexOf('?');

            if ( questionMarkIndex == -1 )
            {
                return parameter;
            }

			string param = url.Substring(questionMarkIndex + 1, url.Length - questionMarkIndex - 1);

			string[] keyValuePairs = param.Split('&');

			foreach (string keyValuePair in keyValuePairs)
			{
				string[] split = keyValuePair.Split("=");
				string key = split[0].ToLower();

                if ( split.Length == 2 )
                    parameter[key] = Uri.UnescapeDataString( split[1] );
                else
                    parameter[key] = null;
			}

			return parameter;
		}

        public static string ForwardParamsToUrl( string sourceUrl, string targetUrl )
        {
            Dictionary < string, string > sourceDict = GetParamsFromUrl( sourceUrl );
            if ( sourceDict.Count == 0 )
            {
                return CleanUrl(targetUrl);
            }

            string result = CleanUrl(targetUrl) + "?";
            bool first = true;
            foreach ( KeyValuePair<string, string> kvp in sourceDict )
            {
				if (first)
                    result += kvp.Key + "=" + kvp.Value;
				else
                    result += "&" + kvp.Key + "=" + kvp.Value;
				first = false;
            }

            return result;
        }

		public static string AddParamsToUrl(string url, PortalParams[] parameter)
		{
			if (parameter.Length == 0)
				return url;

			StringBuilder stringBuilder = new StringBuilder(url);

			bool firstParam = !url.Contains('?');
			foreach (PortalParams param in parameter)
			{
				if (firstParam)
				{
					stringBuilder.Append($"?{param.key}");
					firstParam = false;
				}
				else
					stringBuilder.Append($"&{param.key}");

				if (param.value != null)
					stringBuilder.Append($"={Uri.EscapeDataString(param.value)}");
			}

			return stringBuilder.ToString();
		}

		public static bool CompareDomains(string completeUrl1, string completeUrl2)
		{
            try
            {
                Uri uri1 = new Uri( completeUrl1 );
                Uri uri2 = new Uri( completeUrl2 );
                return uri1.Authority.ToLower() == uri2.Authority.ToLower();
            }
            catch
            {
                return completeUrl1 == completeUrl2;
            }
		}

		public static string UriVRMLFileEndingCompletion(string uri)
		{
			if (string.IsNullOrEmpty(uri))
				return uri;

            if ( !uri.EndsWith( ".vrml" ) )
            {
                if ( uri.EndsWith( "/" ) )
                    uri += "index.vrml";
            }

            return uri;
		}
	}
}