using UnityEngine;
using VRWeb.Utils;

namespace VRWeb.UI
{
	public class IconProvider : SingletonMonoBehaviour < IconProvider >
    {
        // Serialized variables
        [SerializeField]
        private Sprite m_DefaultProcess = null;

        [SerializeField]
        private Sprite m_SuccessfulProcess = null;

        [SerializeField]
        private Sprite m_FailedProcess = null;

        [SerializeField]
        private Sprite m_CanceledProcess = null;

        [SerializeField]
        private Sprite m_DownloadProcess = null;

        [SerializeField]
        private Sprite m_LoadPortalProcess = null;

        [SerializeField]
        private Sprite m_LoadSceneProcess = null;

        // Getters
        public Sprite DefaultProcess => m_DefaultProcess;
        public Sprite SuccessfulProcess => m_SuccessfulProcess;
        public Sprite FailedProcess => m_FailedProcess;
        public Sprite CanceledProcess => m_CanceledProcess;
        public Sprite DownloadProcess => m_DownloadProcess;
        public Sprite LoadPortalProcess => m_LoadPortalProcess;
        public Sprite LoadSceneProcess => m_LoadSceneProcess;

    }

}