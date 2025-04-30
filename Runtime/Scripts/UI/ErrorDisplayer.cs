using System.Collections;
using TMPro;
using UnityEngine;
using VRWeb.Managers;
using VRWeb.Tracking;

namespace VRWeb.UI
{
	public class ErrorDisplayer : MonoBehaviour
	{
		// Public variables
		[SerializeField] private TMP_Text m_Code;
		[SerializeField] private TMP_Text m_Message;
		[SerializeField] private TMP_Text m_ReturnUrl;
        [SerializeField] private int m_ReturnCoundown = 10;
        // Public functions
        public void SetErrorMessage(int code, string message)
		{
			m_Code.text = code.ToString();
			m_Message.text = message;
            StartCoroutine(ReturnToLastUrlCoroutine());
        }

        public IEnumerator ReturnToLastUrlCoroutine()
        {
            TrackHistory manager = HopperRoot.Get<TrackHistory>();

            if ( manager.Last() == null )
                yield break;

            int i = m_ReturnCoundown;
            while(i-- > 0)
            {
                m_ReturnUrl.text = $"Hop to <b>{manager.Last().Link}</b>\nin {i} seconds";

                yield return new WaitForSeconds( 1 );
            }
            HopperRoot.Get<PortalManager>().LoadPortal(manager.Last().Link);
        }
    }
}