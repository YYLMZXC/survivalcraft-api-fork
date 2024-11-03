using Engine;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public abstract class VersionConverter
	{
		public abstract string SourceVersion{get;}

		public abstract string TargetVersion{get;}

		public abstract void ConvertProjectXml(XElement projectNode);

		public abstract void ConvertWorld(string directoryName);
	}
	/*
	public class GenericVersionConverter(string sourceVersion,string targetVersion) : VersionConverter
	{
		private string _sourceVersion = sourceVersion;
		private string _targetVersion = targetVersion;

		public override string SourceVersion => _sourceVersion;

		public override string TargetVersion => _targetVersion;

		public override void ConvertProjectXml(XElement projectNode)
		{
			XmlUtils.SetAttributeValue(projectNode,"Version",TargetVersion);
		}

		public override void ConvertWorld(string directoryName)
		{
			string path = Storage.CombinePaths(directoryName,"Project.xml");
			XElement xElement;
			using(Stream stream = Storage.OpenFile(path,OpenFileMode.Read))
			{
				xElement = XmlUtils.LoadXmlFromStream(stream,null,throwOnError: true);
			}
			ConvertProjectXml(xElement);
			using(Stream stream2 = Storage.OpenFile(path,OpenFileMode.Create))
			{
				XmlUtils.SaveXmlToStream(xElement,stream2,null,throwOnError: true);
			}
		}
	// Example usage:  
	// var converter = new GenericVersionConverter("1.11", "1.12");  
	// converter.ConvertWorld("path_to_world_directory");  
}*/
}
