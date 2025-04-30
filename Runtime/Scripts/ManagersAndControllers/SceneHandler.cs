using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Creator;
using VRWeb.Rig;
using VRWeb.UI;

namespace VRWeb.Managers
{
	public class SceneHandler : MonoBehaviour
	{
		// Singleton instance
		public static SceneHandler Instance { get; private set; }

		// Public variables
		public const string BOOT_SCENE = "BootScene";
		public const string RIG_SCENE = "RigScene";
		public const string PERSISTENT_SCENE = "PersistentScene";
		public const string LOADING_SCENE = "LoadingScene";
		public const string SETTINGS_ROOM = "SettingsRoom";
		public const string ERROR_SCENE = "ErrorScene";

		// Awake function
		private void Awake()
		{
			if (Instance == null)
				Instance = this;
		}

		// OnDestroy function
		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		// Public functions
		public void RestartApp() => EnterBootScene();

		public void EnterBootScene() => SceneManager.LoadScene(BOOT_SCENE);

		public void EnterSettingsRoom()
		{
			EnterLocalScene(SETTINGS_ROOM);
		}

		public void EnterLocalScene(string sceneName)
		{
			StartCoroutine(LoadLocalSceneCoroutine(sceneName));
		}

		public void EnterErrorScene(int errorCode, string errorMessage)
		{
			StartCoroutine(EnterErrorSceneCoroutine(errorCode, errorMessage));
		}

		public IEnumerator RemoveAllPortalScenes()
		{
			yield return RemoveAllScenesExcept(new string[] { PERSISTENT_SCENE, RIG_SCENE, LOADING_SCENE });
		}

		public IEnumerator RemoveAllScenesExcept(string[] scenesToKeep)
		{
			List<UnityEngine.SceneManagement.Scene> scenesToRemove = new List<UnityEngine.SceneManagement.Scene>();

			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				UnityEngine.SceneManagement.Scene s = SceneManager.GetSceneAt(i);

				if (s.isLoaded && !scenesToKeep.Contains(s.name))
					scenesToRemove.Add(s);
			}

			foreach (UnityEngine.SceneManagement.Scene scene in scenesToRemove)
			{
				yield return SceneManager.UnloadSceneAsync(scene);
			}
		}

		public IEnumerator EnterLoadingScene()
		{
			UnityEngine.SceneManagement.Scene loadingScene = SceneManager.GetSceneByName(LOADING_SCENE);
			if (loadingScene.buildIndex < 0)
			{
				yield return SceneManager.LoadSceneAsync(LOADING_SCENE, LoadSceneMode.Additive);
			}

			loadingScene = SceneManager.GetSceneByName(LOADING_SCENE);
			if (!loadingScene.isLoaded)
			{
				yield break;
			}
		}

		public IEnumerator ExitLoadingScene()
		{
			UnityEngine.SceneManagement.Scene loadingScene = SceneManager.GetSceneByName(LOADING_SCENE);

			yield return SceneManager.UnloadSceneAsync(loadingScene);
		}

		// Private functions
		private void SetPositionOnSceneLoad(UnityEngine.SceneManagement.Scene scene)
		{
			PortalTarget portalTarget = GameObject.FindAnyObjectByType<PortalTarget>();

            if (portalTarget != null)
				portalTarget.OnEnterWorld.Invoke();
			else
                HopperRoot.Get < RigPositioner >().
                           SetUserPositionAndForwardDirection( Vector3.zero, Vector3.forward );

            Debug.Log($"Scene loading completed: {scene.name}");
		}

		// Coroutine
		private IEnumerator LoadLocalSceneCoroutine(string sceneName)
		{
			yield return EnterLoadingRoomCoroutine();

			yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

			UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(sceneName);

			if (!scene.IsValid())
			{
				Debug.LogError($"Attempt to load local scene \"{sceneName}\" failed: scene not found.");

				yield break;
			}

			SceneManager.SetActiveScene(scene);

			SetPositionOnSceneLoad(scene);

			yield return ExitLoadingScene();
		}

		private IEnumerator EnterLoadingRoomCoroutine()
		{
			yield return EnterLoadingScene();
			yield return RemoveAllScenesExcept(new string[] { PERSISTENT_SCENE, RIG_SCENE, LOADING_SCENE });

		}
		private IEnumerator EnterErrorSceneCoroutine(int errorCode, string errorMessage)
		{
			yield return EnterLoadingRoomCoroutine();

			yield return SceneManager.LoadSceneAsync(ERROR_SCENE, LoadSceneMode.Additive);

			UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneByName(ERROR_SCENE);

			if (!scene.IsValid())
			{
				Debug.LogError($"Attempt to load local scene \"{ERROR_SCENE}\" failed: scene not found.");

				yield break;
			}

			SceneManager.SetActiveScene(scene);
			SetPositionOnSceneLoad(scene);

			ErrorDisplayer errorDisplayer = GameObject.FindAnyObjectByType<ErrorDisplayer>();
			errorDisplayer?.SetErrorMessage(errorCode, errorMessage);

			yield return ExitLoadingScene();
		}

	}
}