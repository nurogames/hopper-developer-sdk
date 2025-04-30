using UnityEngine;
using VRWeb.Managers;
using VRWeb.UI.Elements;
using VRWeb.User;

namespace VRWeb.UI
{

	public class PopupTester : MonoBehaviour
    {
        //Serialized
        private PopupManager m_PopupManager;

        private Popup m_Popup = null;


        void Awake()
        {
            UserSettings.global = new UserSettings();
        }

        // Start
        void Start()
        {
            m_Popup = new Popup("Test Popup", "Very testy stuff", new Button[]
            {
                Button.CreateYesTextButton(() => m_Popup.Hide()),
                Button.CreateNoTextButton( () => { }),
                Button.CreateCancelIconButton(() => m_Popup.Hide())
            });

            m_PopupManager = HopperRoot.Get<PopupManager>();
        }


        // Update is called once per frame
        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        m_Popup?.Show();
        //    }


        //    if (Input.GetKeyDown(KeyCode.Return))
        //    {
        //        m_Popup?.Hide();
        //    }

        //    if (m_Popup == null || m_PopupManager == null)
        //    {
        //        return;
        //    }
        //}
    }

}