using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
namespace Engine.Input
{
	public class KeyboardInput
	{
		public static List<char> Chars = new List<char>();
		public static bool _DeletePressed;
		public static bool DeletePressed
        {
			get { bool D = _DeletePressed;if (D) _DeletePressed = false; return D; }
            set { _DeletePressed = value; }
		}
		public static string GetInput()
		{
			if (Chars.Count > 0) {
				string str = new string(Chars.ToArray());
				Chars.Clear();
				return str;
			}
			return string.Empty;
		}
	}
}
