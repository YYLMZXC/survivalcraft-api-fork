using System;

namespace Engine.Serialization
{
	[AttributeUsage(AttributeTargets.Class)]
	public class HumanReadableConverterAttribute : Attribute
	{
		public Type Type;

		public HumanReadableConverterAttribute(Type type)
		{
			Type = type;
		}
		public HumanReadableConverterAttribute(Type[] type)
		{
			if (type != null && type.Length > 0)
				Type = type[0];
		}
	}
}
