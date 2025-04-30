using System;
using UnityEngine;

namespace VRWeb.UI.Elements
{

	public class Button
    {
        // Private variables
        private Sprite m_Icon = null;
        private string m_Text = null;

        private ButtonStyle m_Style = null;

        // Event
        public event Action OnInvoke;
        
        // Getter
        public bool IsIconButton => m_Icon != null;
        public bool IsTextButton => m_Text != null;

        public Sprite Icon => m_Icon;
        public string Text => m_Text;

        public ButtonStyle Style => m_Style;

        // Constructor
        private Button( Sprite icon, string text, ButtonStyle style, Action onInvoke )
        {
            m_Icon = icon;
            m_Text = text;
            m_Style = style;
            OnInvoke = onInvoke;
        }

        // Factory functions
        public static Button CreateTextButton(string text, Action onInvoke = null, ButtonStyle style = null) => new Button(null, text, style, onInvoke);
        public static Button CreateOkTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "OK", style, onInvoke);
        public static Button CreateCancelTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "Cancel", style, onInvoke);
        public static Button CreateYesTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "Yes", style, onInvoke);
        public static Button CreateNoTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "No", style, onInvoke);
        public static Button CreateNextTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "Next", style, onInvoke);
        public static Button CreatePreviousTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "Previous", style, onInvoke);
        public static Button CreateForwardTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "Forward", style, onInvoke);
        public static Button CreateBackTextButton(Action onInvoke = null, ButtonStyle style = null) => new Button(null, "Back", style, onInvoke);

        public static Button CreateIconButton(Sprite icon, Action onInvoke = null, ButtonStyle style = null) => new Button(icon, null, style, onInvoke);
        public static Button CreateCheckmarkButton(Action onInvoke = null, ButtonStyle style = null) => new Button(PopupIconProvider.Instance.Checkmark, null, style, onInvoke);
        public static Button CreateCancelIconButton(Action onInvoke = null, ButtonStyle style = null) => new Button(PopupIconProvider.Instance.Cancel, null, style, onInvoke);

        // Public function
        public void Invoke()
        {
            OnInvoke?.Invoke();
        }
    }

}