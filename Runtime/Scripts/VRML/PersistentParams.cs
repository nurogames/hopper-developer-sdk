using System.Collections.Generic;
using VRWeb.Utils;

namespace VRWeb.Core.Settings
{
	public class PersistentParams
	{
		// Private variables
		private Dictionary<string, string> m_Params = new Dictionary<string, string>();

        public Dictionary < string, string > Params => m_Params;

        /// <summary>
        /// scans url and adds all parameters to PersistentParams.<br/>
        /// Overwrites previous parameters of same name
        /// </summary>
        /// <param name="url"></param>
		public void FetchParamsFromUrl( string url)
		{
			Dictionary<string, string> urlsParams = UrlHelper.GetParamsFromUrl(url);

			foreach (KeyValuePair<string, string> kvp in urlsParams)
			{
				string key = kvp.Key.ToLower();
				m_Params[key] = kvp.Value;
			}
		}

		public void ClearAllParams()
        {
            m_Params.Clear();
        }

        public void ClearParam( string key )
        {
            if ( m_Params == null )
            {
				m_Params = new Dictionary<string, string>();
				return;
            }

            key = key.ToLower();
            if ( m_Params.ContainsKey( key ) )
				m_Params.Remove( key );
        }

        public string GetParamAsString(string key, string defaultValue = "")
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				return m_Params[key];

			return defaultValue;
		}

		public void AddStringParam(string key, string value)
        {
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				m_Params[key] = value;
			else
				m_Params.Add(key, value);
		}

		public bool GetParamAsBool(string key, bool defaultValue = false)
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				return bool.Parse(m_Params[key]);

			return defaultValue;
		}

		public void AddBoolParam(string key, bool value)
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				m_Params[key] = value.ToString();
			else
				m_Params.Add(key, value.ToString());
		}

		public int GetParamAsInt(string key, int defaultValue = 0)
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				return int.Parse(m_Params[key]);

			return defaultValue;
		}

		public void AddIntParam(string key, int value)
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				m_Params[key] = value.ToString();
			else
				m_Params.Add(key, value.ToString());
		}

		public float GetParamAsFloat(string key, float defaultValue = 0.0f)
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				return float.Parse(m_Params[key]);

			return defaultValue;
		}

		public void AddFloatParam(string key, float value)
		{
            key = key.ToLower();
			if (m_Params.ContainsKey(key))
				m_Params[key] = value.ToString();
			else
				m_Params.Add(key, value.ToString());
		}

        public bool KeyExists( string key )
        {
            key = key.ToLower();

            return m_Params.ContainsKey( key );
        }
	}
}