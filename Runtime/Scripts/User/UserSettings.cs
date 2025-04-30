using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using VRWeb.VRML;

namespace VRWeb.User
{
	[Serializable]
    public class UserSettings
    {
        public string UUID = new Guid().ToString();
        public string FirstName = "";
        public string Lastname = "";
        public string Handle = "";
        public VRMLMetaInfos CurrentMetaInfos;

        public static UserSettings global = new();

        private const string USPREFIX = "settings_";
        public const string HOPPER_PORTAL_URL = "http://experimental.nuromedia.com/index.vrml";

        public static UserSettings Load(string fullPath)
        {
            try
            {
                string fileText = File.ReadAllText(fullPath);
                UserSettings loadedUserSettings = JsonConvert.DeserializeObject<UserSettings>(fileText);

                return loadedUserSettings;
            }
            catch
            {
                return null;
            }
        }

        public static bool Save(UserSettings userSettings)
        {
            string dirPath = FullPath("");
            string fullPath = FullPath(USPREFIX + "_" + userSettings.Handle);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            StreamWriter streamWriter = null;
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Culture = CultureInfo.InvariantCulture;

            streamWriter = File.CreateText(fullPath);
            string jsonString = JsonConvert.SerializeObject(userSettings, Formatting.None, settings);
            streamWriter.Write(jsonString);
            streamWriter.Close();

            return false;
        }

        public static string[] ListUserSettingsFileNames()
        {
            string path = FullPath("");

            try
            {
                return Directory.GetFiles(path, USPREFIX + "*");
            }
            catch (System.Exception)
            {
                return Array.Empty<string>();
            }
        }

        public static string FullPath(string relPath)
        {
            return Path.Join(Application.persistentDataPath, "UserSettings", relPath);
        }
    }
}