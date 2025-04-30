using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace VRWeb.Avatar
{
	[Serializable]
    public class AvatarSettings
    {
        public string PrivateAvatarUri = "";
        public string PrivateAvatarPrefabName = "";
        public float AvatarMoveSpeed = 3.0f;
        public float AvatarLateralSpeed = 3.0f;
        public float AvatarReverseSpeed = 2.0f;
        public float AvatarRotationSpeed = 8;
        public float AvatarCameraTiltSpeed = 8;
        public float AvatarSprintBoost = 2.0f;
        public static AvatarSettings global = new AvatarSettings();

        private const string AVATAR_PREFIX = "avatar_";
        //public const string DEFAULT_AVATAR_NAME = "default avatar";
        public const string AVATAR_2_0_NAME = "avatar 2.0";

        public static AvatarSettings Load(string fullPath)
        {
            try
            {
                string fileText = File.ReadAllText(fullPath);
                AvatarSettings loadedAvatarSettings = JsonConvert.DeserializeObject<AvatarSettings>(fileText);

                return loadedAvatarSettings;
            }
            catch
            {
                return null;
            }
        }

        public static bool Save(AvatarSettings avatarSettings)
        {
            string dirPath = FullPath("");
            string fullPath = FullPath(AVATAR_PREFIX + avatarSettings.PrivateAvatarPrefabName);

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
            string jsonString = JsonConvert.SerializeObject(avatarSettings, Formatting.None, settings);
            streamWriter.Write(jsonString);
            streamWriter.Close();

            return false;
        }


        public static string[] ListAvatarSettingsFileNames()
        {
            string path = FullPath("");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                return Directory.GetFiles(path, AVATAR_PREFIX + "*");
            }
            catch (System.Exception)
            {
                return Array.Empty<string>();
            }
        }

        private static string FullPath(string relPath)
        {
            return Path.Join(Application.persistentDataPath, "AvatarSettings", relPath);
        }

    }
}