using System.Collections.Generic;

namespace Game
{
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
		public List<string> Dependencies = [];
		public override int GetHashCode()
		{
			return HashCode.Combine(Name, PackageName, Version);
		}

		public override bool Equals(object obj) => obj is ModInfo && obj.GetHashCode() == GetHashCode();
	}
}
