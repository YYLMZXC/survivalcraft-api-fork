using Engine.Media;
namespace Game
{
    public class LabelWidget : FontTextWidget
    {
        public static BitmapFont BitmapFont;
        static LabelWidget(){
            BitmapFont = ContentManager.Get<BitmapFont>("Fonts/Pericles");
        }
        public override string Text
        {
            get => base.Text; set
            {
                if (m_text != value && value != null)
                {
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        string[] xp = value.Substring(1, value.Length - 2).Split(new char[] { ':' });
                        m_text = xp.Length == 2 ? LanguageControl.GetContentWidgets(xp[0], xp[1]) : LanguageControl.Get("Usual", value);
                    }
                    else
                    {
                        m_text = LanguageControl.Get("Usual", value);
                    }
                    m_linesSize = null;
                }

            }
        }
        public LabelWidget() : base()
        {
            Font = BitmapFont;
        }
    }
}
