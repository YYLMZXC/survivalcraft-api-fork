﻿using Engine;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Game
{
	public class ShaderCodeManager
	{
		public static string GetFast(string fname)
		{
			string shaderText = string.Empty;
			string[] parameters = fname.Split('.');
			if (parameters.Length > 1)
				shaderText = ModsManager.GetInPakOrStorageFile<string>(parameters[0], parameters[1]);
			return shaderText;
		}

		public static string Get(string fname)
		{
			string shaderText = string.Empty;
			shaderText = GetIncludeText(shaderText, fname, false);
			return shaderText;
		}

		public static string GetExternal(string fname)
		{
			string shaderText = string.Empty;
			shaderText = GetIncludeText(shaderText, fname, true);
			return shaderText;
		}

		public static string GetIncludeText(string shaderText, string includefname, bool external)
		{
			string includeText = string.Empty;
			string shaderTextTemp = string.Empty;
			try
			{
				if (external)
				{
					string path = ModsManager.IsAndroid ? RunPath.AndroidFilePath : "app:/";
					Stream stream = Storage.OpenFile(Storage.CombinePaths(path, includefname), OpenFileMode.Read);
					StreamReader streamReader = new(stream);
					shaderTextTemp = streamReader.ReadToEnd();
				}
				else
				{
					if (includefname.Contains(".txt"))
					{
						includefname = includefname.Split(new char[1] { '.' })[0];
						shaderTextTemp = ContentManager.Get<string>(includefname);
					}
					else
					{
						shaderTextTemp = GetFast(includefname);
					}
				}
				if (shaderTextTemp == string.Empty) return string.Empty;
				shaderTextTemp = shaderTextTemp.Replace("\n", "$");
				string[] lines = shaderTextTemp.Split(new char[1] { '$' }, StringSplitOptions.RemoveEmptyEntries);
				for (int l = 0; l < lines.Length; l++)
				{
					lines[l] = lines[l].Trim();
					if (lines[l].StartsWith("//"))
					{
						string text = lines[l].Substring(2).TrimStart();
						if (text.StartsWith("<") && text.EndsWith("/>"))
						{
							includeText += lines[l] + "\n";
							continue;
						}
					}
					string[] arline = lines[l].Replace("//", "$").Split(new char[1] { '$' });
					if (arline.Length > 0)
					{
						lines[l] = arline[0];
					}
					if (lines[l].StartsWith("#include"))
					{
						Regex regex = new("\"[^\"]*\"");
						string fname = regex.Match(lines[l]).Value.Replace("\"", "");
						includeText += GetIncludeText(shaderText, fname, external);
					}
					else
					{
						if (!ModsManager.IsAndroid)
						{
							includeText += lines[l].Replace("highp", "").Replace("lowp", "").Replace("mediump", "") + "\n";
						}
						else
						{
#pragma warning disable CS0162 // 检测到无法访问的代码
							includeText += lines[l] + "\n";
#pragma warning restore CS0162 // 检测到无法访问的代码
						}
					}
				}
				shaderText += includeText;
			}
			catch
			{
			}
			return shaderText;
		}
	}
}