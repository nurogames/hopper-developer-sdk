using UnityEngine;

namespace VRWeb.Utils
{
    public static class ApplicationHelper
    {
        public static void MinimizeApplication()
        {
            Debug.LogError("Not implemented!");
        }

        public static void CloseApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
