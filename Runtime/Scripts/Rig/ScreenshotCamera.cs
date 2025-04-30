using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRWeb.Rig
{
	[RequireComponent(typeof(Camera))]
    public class ScreenshotCamera : MonoBehaviour
    {
        private Camera m_Camera;
        private Texture2D m_Texture = null;
        private string m_FileName;

        #region Unity Event Functions

        #endregion

        #region Public

        public Texture2D Screenshot => m_Texture;

        public void TakeScreenshot(Vector2Int resolution, string fileName = null)
        {
            m_Camera = GetComponent<Camera>();
            m_FileName = fileName;

            m_Camera.enabled = false;

            m_Camera.targetTexture = new RenderTexture(
                resolution.x,
                resolution.y,
                24,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.sRGB);

            StartCoroutine(TakeScreenshotCoroutine());
        }

        #endregion

        #region Private

        private IEnumerator TakeScreenshotCoroutine()
        {
            m_Camera.enabled = true;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            m_Camera.enabled = false;

            yield return AsyncGPUReadback.Request(
                m_Camera.targetTexture,
                0,
                GetScreenshotFromTexture);
        }

        private void GetScreenshotFromTexture(AsyncGPUReadbackRequest request)
        {
            m_Texture = new Texture2D(
                m_Camera.targetTexture.width,
                m_Camera.targetTexture.height,
                TextureFormat.RGBA32,
                false
            );

            m_Texture.LoadRawTextureData(request.GetData<uint>());
            m_Texture.Apply();

            if (m_FileName != null)
            {
                SaveScreenshot();
            }
            else
            {
                Debug.Log("Screenshot created");
            }
        }

        private void SaveScreenshot()
        {
            string path = Path.Join(Application.persistentDataPath, "snapshots", m_FileName) + ".jpg";
            File.WriteAllBytes(path, ImageConversion.EncodeToJPG(m_Texture));
            Debug.Log($"Screenshot saved as {path}");
        }

        #endregion
    }
}