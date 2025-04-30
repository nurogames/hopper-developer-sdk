using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Managers;

namespace VRWeb.Utils
{
	public class EnsureRigsIsLoaded : MonoBehaviour
	{
		IEnumerator Start()
		{
			UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName( SceneHandler.RIG_SCENE);

			if (!scene.isLoaded)
			{
				yield return SceneManager.LoadSceneAsync( SceneHandler.RIG_SCENE, LoadSceneMode.Additive);
			}
		}
	}
}