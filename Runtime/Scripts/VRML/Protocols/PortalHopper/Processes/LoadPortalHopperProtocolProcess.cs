using Nuro.Processes;
using VRWeb.Processes;

namespace VRWeb.VRML.Protocols.Processes
{
	public class LoadPortalHopperProtocolProcess : ContainerProcess
    {
        public LoadPortalHopperProtocolProcess(Portal portal, VRMLFile_PortalHopperProtocol protocol) : base("Load Portal Hoper Protocol")
		{
			bool replace = protocol.LoadMode == VRMLFile_PortalHopperProtocol.LoadModes.Replace;
			Process previous = null;
			if(replace)
			{
				Process enterLoadingSceneProcess = new EnterLoadingSceneProcess();
				Add(enterLoadingSceneProcess);

				previous = new RemoveAllPortalScenesProcess();
				Add(previous, enterLoadingSceneProcess);
			}

            LoadAllRequiredDllsProcess loadAllRequiredDllsProcess = null;
            if ( protocol.DllPaths != null && protocol.DllPaths.Count > 0 )
            {
                loadAllRequiredDllsProcess =
                    new LoadAllRequiredDllsProcess( protocol, protocol.DllPaths.ToArray() );

                Add( loadAllRequiredDllsProcess, previous );
            }

            string sceneAssetBundlePath = protocol.GetPatchedPath(portal.Url);
			LoadAllRequiredAssetBundlesProcess loadAllRequiredAssetBundlesProcess = 
                new LoadAllRequiredAssetBundlesProcess(null, sceneAssetBundlePath, protocol.VersionAttribute);
			Add(loadAllRequiredAssetBundlesProcess);

			bool isMain = protocol.IsMain;

			LoadPortalHopperSceneProcess loadSceneProcess = 
                new LoadPortalHopperSceneProcess(
					portal.Url,
					protocol,
                    sceneAssetBundlePath, 
                    protocol.ScenePath,
                    protocol.IsMain);
			Add(loadSceneProcess, new Process[] { loadAllRequiredDllsProcess, loadAllRequiredAssetBundlesProcess });

			if (replace)
			{
				Add(new ExitLoadingSceneProcess(), loadSceneProcess);
			}
		}
    }
}