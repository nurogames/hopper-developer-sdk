using Nuro.Processes;
using System.Collections.Generic;
using VRWeb.Developer;
using VRWeb.Managers;
using VRWeb.UI;

namespace VRWeb.Processes
{

	public class LoadAllRequiredAssetBundlesProcess : ContainerProcess
    {
        public class AssetBundleData
        {
            public string path;
            public uint version;
        }

		public LoadAllRequiredAssetBundlesProcess( AssetBundleData[] preloadAssetBundleData, string sceneAssetBundlPath, uint sceneVersion) :
			base("Load All Assets for ", IconProvider.Instance.DownloadProcess)
		{
			List<Process> requiredProcesses = new List<Process>();
			UrlMapper mapper = HopperRoot.Get<UrlMapper>();
			AssetBundleManager bundleManager = HopperRoot.Get<AssetBundleManager>();

			if (preloadAssetBundleData != null)
			{
				foreach (AssetBundleData portalPreloadContainerData in preloadAssetBundleData)
				{
					string mappedContainerPath = mapper.MappedUrl(portalPreloadContainerData.path);
					if (bundleManager.IsAssetBundleLoaded(mappedContainerPath))
						continue;

					Process process = bundleManager.CreateDownloadAndCacheAssetBundleProcess(
                        mappedContainerPath,
                        portalPreloadContainerData.version );

					requiredProcesses.Add(process);
					Add(process);
				}
			}

			string mappedSceneContainerPath = mapper.MappedUrl(sceneAssetBundlPath);
			if (!bundleManager.IsAssetBundleLoaded(mappedSceneContainerPath))
			{
				Process loadSceneAssetBundleProcess = bundleManager.CreateDownloadAndCacheAssetBundleProcess(
                    mappedSceneContainerPath,
                    sceneVersion );

				Add(loadSceneAssetBundleProcess, requiredProcesses);
			}
		}
	}

}