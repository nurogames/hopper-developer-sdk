using VRWeb.Managers;
using VRWeb.UI.Elements;

namespace VRWeb.UI
{

	public class Popup
    {
        // Private variables
        private string m_Title = "New Popup";
        private string m_Text = "Important text!!!!";

        private Button[] m_Buttons;

        private PopupStyle m_Style = null;

        // Getter
        public string Title => m_Title;
        public string Text => m_Text;

        public Button[] Buttons => m_Buttons;

        public PopupStyle Style => m_Style;

        // Private variable
        private PopupPriorities m_Priority = PopupPriorities.Info;

        // Getter
        public PopupPriorities Priority => m_Priority;

        // Constructor
        public Popup( string title, string text, Button[] buttons, PopupStyle style = null ) : this(PopupPriorities.Info, title, text, buttons, style )
        {
            // Nothing to do
        }

        public Popup( PopupPriorities priority, string title, string text, Button[] buttons, PopupStyle style = null)
        {
            m_Priority = priority;
            m_Title = title;
            m_Text = text;
            m_Buttons = buttons;
            m_Style = style;

            foreach (Button button in m_Buttons)
            {
                button.OnInvoke += Hide;
            }
        }

        // Public functions
        public void Show()
        {
            HopperRoot.Get<PopupManager>().ShowPopup( this );
        }

        public void Hide()
        {
            HopperRoot.Get<PopupManager>().RemovePopup( this );
        }
    }

}