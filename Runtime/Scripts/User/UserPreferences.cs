using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using VRWeb.Avatar;

namespace VRWeb.User
{
	[Serializable]
    public class UserPreferences
    {
        public string UUID = new Guid().ToString();
        public float PopupPlacementDistance = 0.5f;
        public float PopupPlacementHeight = 0.2f;
        public float PopupFollowSpeed = 1f;

        public Vector3 LeftFingerLocalPosition = Vector3.zero;
        public Quaternion LeftFingerLocalRotation = Quaternion.identity;

        public Vector3 LeftWatchLocalPosition = Vector3.zero;
        public Quaternion LeftWatchLocalRotation = Quaternion.identity;

        public Vector3 RightFingerLocalPosition = Vector3.zero;
        public Quaternion RightFingerLocalRotation = Quaternion.identity;

        public Vector3 RightWatchLocalPosition = Vector3.zero;
        public Quaternion RightWatchLocalRotation = Quaternion.identity;

        public Vector3 CalibrateHeadPosition = new Vector3(0, 1.70f, 0);
        public Vector3 CalibrateLeftHandPosition = new Vector3(-1, 0, 0);
        public Vector3 CalibrateRightHandPosition = new Vector3(1, 0, 0);

        private static UserPreferences globalInstance = null;

        [JsonIgnore]
        public static UserPreferences global => globalInstance == null ? UserPreferences.Load() : globalInstance;

        public static UserPreferences Load()
        {
            string path = UserSettings.FullPath("preferences.json");

            try
            {
                string fileText = File.ReadAllText(path);
                globalInstance = JsonConvert.DeserializeObject<UserPreferences>(fileText);
            }
            catch
            {
                globalInstance = new UserPreferences();
            }

            return globalInstance;
        }

        public static void Save()
        {
            if (global == null)
            {
                return;
            }

            string path = UserSettings.FullPath("");
            string filePath = UserSettings.FullPath("preferences.json");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            StreamWriter streamWriter = null;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Culture = CultureInfo.InvariantCulture;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            streamWriter = File.CreateText(filePath);
            string jsonString = JsonConvert.SerializeObject(global, Formatting.None, settings);
            streamWriter.Write(jsonString);
            streamWriter.Close();
        }
    }
}