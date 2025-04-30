using Nuro.Processes;
using System.Collections.Generic;
using VRWeb.Managers;
using VRWeb.UI;
using VRWeb.VRML.Protocols;

namespace VRWeb.Processes
{
	public class LoadAllRequiredDllsProcess : ContainerProcess
    {
        public LoadAllRequiredDllsProcess( VRMLFile_PortalHopperProtocol protocol, string[] dllPaths) :
			base("Load All DLLs for ", IconProvider.Instance.DownloadProcess)
		{
			List<Process> requiredProcesses = new List<Process>();

			if (dllPaths != null && dllPaths.Length > 0)
			{
				foreach (string preloadDllPath in dllPaths)
				{
					string dllPath = protocol.GetPatchedPath( preloadDllPath );

                    if (HopperRoot.Get<DllCache>().IsDllCached( dllPath ))
						continue;

					Process process = HopperRoot.Get<DllCache>().CreateDownloadDllProcess( dllPath );

					requiredProcesses.Add(process);
					Add(process);
				}
			}
		}
	}
}
