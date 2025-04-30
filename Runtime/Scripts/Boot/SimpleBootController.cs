using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRWeb.Managers;

public class SimpleBootController : MonoBehaviour
{
    [SerializeField]
    private string m_InitialVrmlFileURL = null;

    IEnumerator Start()
    {
        yield return new WaitUntil( () => HopperRoot.Get < PortalManager >() != null );

        Scene scene = SceneManager.GetSceneByName( SceneHandler.RIG_SCENE );

        while( !scene.isLoaded )
        {
            yield return null;
        }

        HopperRoot.Get < PortalManager >().LoadPortal( m_InitialVrmlFileURL );
    }

}
