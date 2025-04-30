using Nuro.Processes;
using System.Collections;
using VRWeb.Managers;

namespace VRWeb.Processes
{
	public class ExitLoadingSceneProcess : Process
	{
		protected override IEnumerator ProcessFunc()
		{
			yield return SceneHandler.Instance.ExitLoadingScene();
		}
	}

}