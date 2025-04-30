using System.Collections;
using UnityEngine;

namespace VRWeb.Utils
{
	/// <summary>
	/// add this component to any prefabs with special shaders in case you have
	/// problems displaying them properly. This fixes an error where shaders are not running
	/// properly when loaded inside of a prefab from an asset bundle
	/// </summary>
	public class ReloadShader : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null; // wait for object to be fully loaded

            Renderer[] rendererArray = GetComponentsInChildren < Renderer >( true );

            foreach ( Renderer r in rendererArray )
            {
                StartCoroutine( FixShaders.FixShaderCoroutine( r ) );
            }
        }
    }
}