using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.Input
{
	public class KeyboardInput
	{
		[DllImport("Imm32.dll")]
		public static extern IntPtr ImmGetContext(IntPtr hWnd);
		[DllImport("Imm32.dll")]
		public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		private static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);
		//这里先说明一下，以输入“中国”为例
		//切换到中文输入法后，输入“zhongguo”，这个字符串称作IME组成字符串
		//而在输入法列表中选择的字符串“中国”则称作IME结果字符串
		public enum GCS
		{
			RESULTSTR = 0x0800,//返回结果串
			COMPREADSTR = 0x0001,//当前输入串
			COMPSTR = 0x0008//组成字符串
		}
		public static string GetInput()
		{
			IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
			IntPtr hIMC = ImmGetContext(handle);
			try
			{
				int strLen = ImmGetCompositionStringW(hIMC, (int)GCS.COMPSTR, null, 0);
				if (strLen > 0)
				{
					byte[] buffer = new byte[strLen];
					ImmGetCompositionStringW(hIMC, (int)GCS.COMPSTR, buffer, strLen);
					return Encoding.Unicode.GetString(buffer);
				}
				else
				{
					return string.Empty;
				}
			}
			finally
			{
				ImmReleaseContext(handle, hIMC);
			}
		}
	}
}
