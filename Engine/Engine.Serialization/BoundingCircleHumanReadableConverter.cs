using System;

namespace Engine.Serialization
{
	[HumanReadableConverter(typeof(BoundingCircle))]
    public class BoundingCircleHumanReadableConverter : IHumanReadableConverter
	{
		public string ConvertToString(object value)
		{
			var boundingCircle = (BoundingCircle)value;
			return HumanReadableConverter.ValuesListToString<float>(',', boundingCircle.Center.X, boundingCircle.Center.Y, boundingCircle.Radius);
		}

		public object ConvertFromString(Type type, string data)
		{
			float[] array = HumanReadableConverter.ValuesListFromString<float>(',', data);
            return array.Length == 3 ? (object)new BoundingCircle(new Vector2(array[0], array[1]), array[2]) : throw new Exception();
        }
    }
}
