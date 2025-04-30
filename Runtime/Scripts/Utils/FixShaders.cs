using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRWeb.Utils
{
	public class FixShaders : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded( Scene loadedScene, LoadSceneMode mode )
        {
            StartCoroutine( SceneLoadedCoroutine( loadedScene, mode) );
        }

        private IEnumerator SceneLoadedCoroutine( Scene loadedScene, LoadSceneMode mode )
        {
            yield return null;
            yield return null;
            yield return null;
    
            
            if (Application.platform != RuntimePlatform.Android)
            {
                Renderer[] rendererArray = FindObjectsByType < Renderer >(FindObjectsSortMode.None);

                foreach ( Renderer r in rendererArray )
                {
                    if ( r.gameObject.scene != loadedScene )
                        continue;

                    StartCoroutine( ( IEnumerator )FixShaderCoroutine( r ) );
                }

                Terrain[] terrains = FindObjectsByType < Terrain >(FindObjectsSortMode.None);

                foreach ( var terrain in terrains )
                {
                    if ( terrain.gameObject.scene != loadedScene || terrain.materialTemplate.shader == null )
                        continue;

                    Shader shader = Shader.Find( terrain.materialTemplate.shader.name );

                    if ( shader != null )
                        terrain.materialTemplate.shader = shader;
                }
            }
        }

        public static IEnumerator FixShaderCoroutine( Renderer rendererToFix )
        {
            foreach ( Material m in rendererToFix.sharedMaterials )
            {
                if ( m != null )
                {
                    Shader shader = Shader.Find( m.shader.name );

                    if ( shader != null )
                    {
                        m.shader = shader;

                        yield return null;
                    }
                }
            }

        }
    }
}