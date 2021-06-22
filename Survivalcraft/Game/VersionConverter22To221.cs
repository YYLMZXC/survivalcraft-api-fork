using Engine;
using Engine.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
    public class VersionConverter22To221:VersionConverter
    {
        public override string SourceVersion => "2.2";
        public override string TargetVersion => "2.21";
        public override void ConvertProjectXml(XElement projectNode)
        {
            projectNode.Attribute("Version").Value = TargetVersion;
        }
        public override void ConvertWorld(string directoryName)
        {
            string path = Storage.CombinePaths(directoryName, "Project.xml");
            string path2 = Storage.CombinePaths(directoryName, "Project.Old.xml");
            Storage.MoveFile(path,path2);
            using (Stream stream = Storage.OpenFile(path2, OpenFileMode.Read))
            {
                XElement xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
                ConvertProjectXml(xElement);
                using (Stream stream2 = Storage.OpenFile(path, OpenFileMode.Create))
                {
                    XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
                }
            }
            Terrain terrain = new Terrain();
            TerrainSerializer22 terrainSerializer22 = new TerrainSerializer22(terrain, directoryName);
            TerrainSerializer221 terrainSerializer221 = new TerrainSerializer221(new Terrain(),directoryName);
            foreach (var key in terrainSerializer22.m_chunkOffsets) {
                TerrainChunk terrainChunk = new TerrainChunk(terrain,key.Key.X,key.Key.Y);
                if (terrainSerializer22.LoadChunk(terrainChunk)) {
                    terrainChunk.State = TerrainChunkState.Valid;
                    terrainChunk.ModificationCounter = 65535;
                    terrainSerializer221.SaveChunk(terrainChunk);
                }
            }
            terrainSerializer22.Dispose();
            terrainSerializer221.Dispose();
            Storage.DeleteFile(path2);
            Storage.DeleteFile(Storage.CombinePaths(directoryName, "Chunks32h.dat"));
        }
    }
}
