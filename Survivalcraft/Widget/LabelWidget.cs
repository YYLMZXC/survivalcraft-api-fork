using Engine;
using Engine.Graphics;
using Engine.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class LabelWidget : FontTextWidget
    {
        public override string Text
        {
            get => base.Text; set
            {
                if (m_text != value && value != null)
                {
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        string[] xp = value.Substring(1, value.Length - 2).Split(new char[] { ':' });
                        if (xp.Length == 2) m_text = LanguageControl.GetContentWidgets(xp[0], xp[1]);
                        else m_text = LanguageControl.Get("Usual", value);
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
            Font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
        }
    }
}
