using Engine.Serialization;
using TemplatesDatabase;
namespace Game
{
	public class WorldPalette
	{
		public const int MaxColors = 16;

		public const int MaxNameLength = 16;

		public static readonly Color[] DefaultColors = new Color[16]
		{
			new(255, 255, 255),
			new(181, 255, 255),
			new(255, 181, 255),
			new(160, 181, 255),
			new(255, 240, 160),
			new(181, 255, 181),
			new(255, 181, 160),
			new(181, 181, 181),
			new(112, 112, 112),
			new(32, 112, 112),
			new(112, 32, 112),
			new(26, 52, 128),
			new(87, 54, 31),
			new(24, 116, 24),
			new(136, 32, 32),
			new(24, 24, 24)
		};

		public Color[] Colors;

		public string[] Names;

		public WorldPalette()
		{
			Colors = DefaultColors.ToArray();
            Names = LanguageControl.jsonNode[GetType().Name]["Colors"].AsArray().Select(x => x.ToString()).ToArray();
        }

		public WorldPalette(ValuesDictionary valuesDictionary)
		{
			string[] array = valuesDictionary.GetValue("Colors", new string(';', 15)).Split(';');
			if (array.Length != 16)
			{
				throw new InvalidOperationException(LanguageControl.Get(GetType().Name, 0));
			}
			Colors = array.Select((string s, int i) => (!string.IsNullOrEmpty(s)) ? HumanReadableConverter.ConvertFromString<Color>(s) : DefaultColors[i]).ToArray();
			string[] array2 = valuesDictionary.GetValue("Names", new string(';', 15)).Split(';');
			if (array2.Length != 16)
			{
				throw new InvalidOperationException(LanguageControl.Get(GetType().Name, 1));
			}
			Names = array2.Select((string s, int i) => (!string.IsNullOrEmpty(s)) ? s : LanguageControl.GetWorldPalette(i)).ToArray();
			string[] names = Names;
			int num = 0;
			while (true)
			{
				if (num < names.Length)
				{
					if (!VerifyColorName(names[num]))
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			throw new InvalidOperationException(LanguageControl.Get(GetType().Name, 2));
		}

		public ValuesDictionary Save()
		{
			var valuesDictionary = new ValuesDictionary();
			string value = string.Join(";", Colors.Select((Color c, int i) => (!(c == DefaultColors[i])) ? HumanReadableConverter.ConvertToString(c) : string.Empty));
			string value2 = string.Join(";", Names.Select((string n, int i) => (!(n == LanguageControl.Get(GetType().Name, i))) ? n : string.Empty));
			valuesDictionary.SetValue("Colors", value);
			valuesDictionary.SetValue("Names", value2);
			return valuesDictionary;
		}

		public void CopyTo(WorldPalette palette)
		{
			palette.Colors = Colors.ToArray();
			palette.Names = Names.ToArray();
		}

		public static bool VerifyColorName(string name)
		{
			if (name.Length < 1 || name.Length > 16)
			{
				return false;
			}
			foreach (char c in name)
			{
				if (!char.IsLetterOrDigit(c) && c != '-' && c != ' ')
				{
					return false;
				}
			}

			return true;
		}
	}
}
