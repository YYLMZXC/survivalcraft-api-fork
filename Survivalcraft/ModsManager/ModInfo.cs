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
					  PackageName,
					  Email;
		public List<string> Dependencies = [];
		public override int GetHashCode() => (PackageName + ApiVersion + Version).GetHashCode();
		public override bool Equals(object obj) => obj is ModInfo && obj.GetHashCode() == GetHashCode();
	}
}
