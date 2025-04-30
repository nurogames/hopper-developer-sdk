using Nuro.Processes;
using VRWeb.UI;
using VRWeb.Utils;

namespace VRWeb.VRML.Processes
{
	public class GetVRMLFileWebRequestProcess : GetWebRequestProcess
	{
		// Private variable
		private VRMLFile m_File = null;

		// Getter
		public VRMLFile File => m_File;

		// Public
		public GetVRMLFileWebRequestProcess(string uri) : base(
			"Fetch VRML-File: " + uri,
			UrlHelper.UriVRMLFileEndingCompletion(UrlHelper.CleanUrl(uri)),
			IconProvider.Instance.DownloadProcess)
		{

		}

		// Protected functions
		protected override void OnSuccess(string requestResult)
		{
			VRMLFile file = new VRMLFile();
			if(file.LoadFromString(requestResult))
				m_File = file;
		}
	}
}
