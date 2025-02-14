using System;

namespace Engine.Serialization
{
	[HumanReadableConverter(typeof(BoundingBox))]
    public class BoundingBoxHumanReadableConverter : IHumanReadableConverter
	{
		public string ConvertToString(object value)
		{
			var boundingBox = (BoundingBox)value;
			return HumanReadableConverter.ValuesListToString<float>(',', boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z, boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z);
		}

		public object ConvertFromString(Type type, string data)
		{
			float[] array = HumanReadableConverter.ValuesListFromString<float>(',', data);
            return array.Length == 6 ? (object)new BoundingBox(array[0], array[1], array[2], array[3], array[4], array[5]) : throw new Exception();
        }
    }
}
