using Engine;
using System;
using System.Xml.Linq;

namespace Game
{
	public class RunJsDialog : Dialog
	{

		public TextBoxWidget m_InputBox;
		public TextBoxWidget m_OutputBox;

		public LabelWidget m_timeCostedLabel;

		public ButtonWidget m_runButtonWidget;
		public ButtonWidget m_closeButtonWidget;

		public RunJsDialog()
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/RunJsDialog");
			LoadContents(this, node);
			m_InputBox = Children.Find<TextBoxWidget>("RunJsDialog.Input");
			m_OutputBox = Children.Find<TextBoxWidget>("RunJsDialog.Output");
			m_timeCostedLabel = Children.Find<LabelWidget>("RunJsDialog.TimeCosted");
			m_runButtonWidget = Children.Find<ButtonWidget>("RunJsDialog.RunButton");
			m_closeButtonWidget = Children.Find<ButtonWidget>("RunJsDialog.CloseButton");
			m_InputBox.HasFocus = true;
			m_InputBox.Enter += delegate
			{
				Dismiss(true);
			};
			m_InputBox.Escape += delegate
			{
				Dismiss(false);
			};
		}

		public override void Update()
		{
			if (Input.Back || Input.Cancel)
			{
				Dismiss(false);
			}
			else if (Input.Ok)
			{
				Dismiss(true);
			}
			else if (m_runButtonWidget.IsClicked)
			{
				Dismiss(true);
			}
			else if (m_closeButtonWidget.IsClicked)
			{
				Dismiss(false);
			}
		}

		public void Dismiss(bool flag)
		{
			if (flag)
			{
				DateTime now = DateTime.Now;
				string result = JsInterface.Evaluate(m_InputBox.Text);
				TimeSpan timeCosted = (DateTime.Now - now);
				m_OutputBox.Text = result;
				m_timeCostedLabel.Text = $"{MathUtils.Floor(timeCosted.TotalSeconds)}s {timeCosted.Milliseconds}ms";
			}
			else
			{
				DialogsManager.HideDialog(this);
			}
		}
	}
}
