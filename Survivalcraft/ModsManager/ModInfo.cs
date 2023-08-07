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
		public List<string> Dependencies = new List<string>();
		public override int GetHashCode() => (PackageName + ApiVersion + Version).GetHashCode();
		public override bool Equals(object obj) => obj is ModInfo && obj.GetHashCode() == GetHashCode();
	}
}
