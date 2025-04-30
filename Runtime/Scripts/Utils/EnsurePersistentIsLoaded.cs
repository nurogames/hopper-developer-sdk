using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Managers;

namespace VRWeb.Utils
{
	public class EnsurePersistentIsLoaded : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null;

            Scene pScene = SceneManager.GetSceneByName( SceneHandler.PERSISTENT_SCENE );

            if ( !pScene.isLoaded )
            {
                yield return SceneManager.LoadSceneAsync( SceneHandler.PERSISTENT_SCENE, LoadSceneMode.Additive );
            }
        }
    }
}