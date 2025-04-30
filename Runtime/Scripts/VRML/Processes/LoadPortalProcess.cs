using Nuro.Processes;
using System.Collections.Generic;

namespace VRWeb.VRML.Processes
{
	public class LoadPortalProcessV2 : ContainerProcess
	{
		public LoadPortalProcessV2(Portal portal) :	base("Load portal " + portal.Url)
		{
			List<VRMLFile_Protocol> protocols = portal.VRMLFile.Protocols;

			Process previousProcess = null;
			foreach (VRMLFile_Protocol protocol in protocols)
			{
				Process process = protocol.CreateLoadProcess(portal);
				if (process != null)
				{
					Add(process, previousProcess);
					previousProcess = process;
				}
			}
		}
	}
}