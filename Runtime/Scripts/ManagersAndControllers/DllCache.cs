using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using VRWeb.Developer;
using VRWeb.Processes;


namespace VRWeb.Managers
{
	public class DllCache : HopperManagerMonoBehaviour<DllCache>
    {
        private const string DLLCACHE_NAME = "DllCache";

        private Dictionary<string, Dictionary<string, bool>> m_CachedDlls =
            new Dictionary<string, Dictionary<string, bool>>();

        private AppDomain m_Domain;

        private void Awake()
        {
            //Todo: Überprüfen
            //AppDomain.CreateDomain( "VrWeb" );
            RegisterManager();
            ScanDllCache();
        }

        public bool IsDllCached(string dllPathFromVRML)
        {
            if (!ExtractDomainAndFilename(dllPathFromVRML, out string domain, out string fileName))
                return false;

            if (!m_CachedDlls.ContainsKey(domain))
                return false;

            return m_CachedDlls[domain].ContainsKey(fileName);
        }

        public bool IsDllLoaded(string dllPathFromVRML)
        {
            if (!ExtractDomainAndFilename(dllPathFromVRML, out string domain, out string fileName))
                return false;

            if (!m_CachedDlls.ContainsKey(domain))
                return false;

            return m_CachedDlls[domain][fileName];
        }

        public bool LoadDll(string dllPathFromVRML)
        {
            if (IsDllLoaded(dllPathFromVRML))
                return true;

            if (!IsDllCached(dllPathFromVRML))
                return false;

            ExtractDomainAndFilename(dllPathFromVRML, out string domain, out string dllName);
            string localDllPath = BuildPathFromDomainAndDllName(domain, dllName);

            Assembly assembly = Assembly.LoadFrom(localDllPath);

            return assembly != null;
        }

        public DownloadDllProcess CreateDownloadDllProcess(string dllPathFromVRML)
        {
            return new DownloadDllProcess(dllPathFromVRML);
        }

        private void ScanDllCache()
        {
            string dllCachePath = GetDllCachePath();

            if (!Directory.Exists(dllCachePath))
            {
                Directory.CreateDirectory(dllCachePath);
                return;
            }

            string[] hostPaths = Directory.GetDirectories(dllCachePath);

            foreach (var hostPath in hostPaths)
            {
                string[] dllPaths = Directory.GetFiles(hostPath);
                string hostName = Path.GetFileName(hostPath).ToLower();

                foreach (var dllPath in dllPaths)
                {
                    string dllName = Path.GetFileName(dllPath).ToLower();
                    RegisterDll(hostName, dllName);
                }
            }
        }

        public void RegisterDll(string dllUrlFromVRML)
        {
            ExtractDomainAndFilename(dllUrlFromVRML, out string hostName, out string dllName);
            RegisterDll(hostName, dllName);
        }

        public void RegisterDll(string hostName, string dllName)
        {
            if (!m_CachedDlls.ContainsKey(hostName))
            {
                Dictionary<string, bool> hostSpecificDict = new();
                m_CachedDlls.Add(hostName, hostSpecificDict);
                hostSpecificDict.Add(dllName, false);
            }
            else if (!m_CachedDlls[hostName].ContainsKey(dllName))
            {
                m_CachedDlls[hostName].Add(dllName, false);
            }
        }

        private string GetDllCachePath()
        {
            return Path.Combine(Application.persistentDataPath, DLLCACHE_NAME);
        }

        public bool ExtractDomainAndFilename(string fullURL, out string domain, out string fileName)
        {
            Uri uri = new Uri(HopperRoot.Get<UrlMapper>().MappedUrl(fullURL));
            domain = uri.Host;
            fileName = uri.AbsolutePath;
            fileName = fileName.Replace("/", "^");
            return !(string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(fileName));
        }

        public string BuildPathFromDomainAndDllName(string domain, string dllName)
        {
            return Path.Join(GetDllCachePath(), domain, dllName);
        }

        public string BuilLocalFilePathFromDomainAndDllName(string domain, string dllName)
        {
            return Path.Join(GetDllCachePath(), domain, dllName);
        }

        public string DllUrlToLocalFilename(string dllPathFromVRML, bool createFolders = false)
        {
            ExtractDomainAndFilename(dllPathFromVRML, out string domain, out string dllName);
            string path = BuildPathFromDomainAndDllName(domain, dllName);

            if (createFolders)
            {
                string pathWithoutFileNname = Path.GetDirectoryName(path);

                if (!Directory.Exists(pathWithoutFileNname))
                    Directory.CreateDirectory(pathWithoutFileNname);
            }

            return path;
        }
    }
}