using System.Collections.Generic;

namespace Game
{
	public enum LoadOrder
	{
		Survivalcraft = -2147483648,
		ThemeMod = -16384,
		Default = 0,
		HelpfulMod = 16384
	}
	public class ModInfo
	{
		public string Name,
					  Version,
					  ApiVersion,
					  Description,
					  ScVersion,
					  Link,
					  Author,
					  PackageName;
		public int LoadOrder = 0;
		public List<string> Dependencies = [];
		public override int GetHashCode()
		{
			return HashCode.Combine(Name, PackageName, Version);
		}

		public override bool Equals(object obj) => obj is ModInfo && obj.GetHashCode() == GetHashCode();
	}
}
