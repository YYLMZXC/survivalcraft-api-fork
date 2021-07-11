using Engine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    public class TerrainSerializer221 : IDisposable
    {
        public Terrain m_terrain;
        public string DirectoryName;
        public static string Key = "2.21";
        public TerrainSerializer221(Terrain terrain, string directoryName)
        {
            m_terrain = terrain;
            directoryName = Storage.CombinePaths(directoryName,"Chunks");
            if (!Storage.DirectoryExists(directoryName)) {
                Storage.CreateDirectory(directoryName);
            }
            DirectoryName = directoryName;

        }
        public bool LoadChunk(TerrainChunk chunk)
        {
            return LoadChunkBlocks(chunk);
        }
        public void SaveChunk(TerrainChunk chunk)
        {
            if (chunk.State > TerrainChunkState.InvalidContents4 && chunk.ModificationCounter > 0)
            {
                SaveChunkBlocks(chunk);
                chunk.ModificationCounter = 0;
            }
        }
        public void Dispose()
        {

        }
        /// <summary>
        /// 把Stream读为Chunk
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool LoadChunkBlocks(TerrainChunk chunk)
        {
            try
            {
                string readFile = Storage.CombinePaths(DirectoryName, $"{Locker.Encrypt(chunk.Coords.ToString())}.dat");
                if (Storage.FileExists(readFile))
                {
                    using (Stream stream = Storage.OpenFile(readFile, OpenFileMode.CreateOrOpen))
                    {
                        using (Stream memory = ModsManager.StreamDecompress(stream))
                        {
                            var binaryReader = new BinaryReader(memory);
                            int CellsCount = binaryReader.ReadInt32();
                            for (int i = 0; i < CellsCount; i++)
                            {
                                chunk.Cells[i] = binaryReader.ReadInt32();
                            }
                            int ShaftsCount = binaryReader.ReadInt32();

                            for (int i = 0; i < ShaftsCount; i++)
                            {
                                chunk.Shafts[i] = binaryReader.ReadInt32();
                            }

                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                return false;
            }

        }
        /// <summary>
        /// 把Chunk储存为Stream
        /// </summary>
        /// <param name="chunk"></param>
        public void SaveChunkBlocks(TerrainChunk chunk)
        {
            string saveFile = Storage.CombinePaths(DirectoryName, $"{Locker.Encrypt(chunk.Coords.ToString())}.dat");
            bool fileExists = Storage.FileExists(saveFile);
            try
            {

                if (fileExists==false)
                {
                    Stream stream = Storage.OpenFile(saveFile, OpenFileMode.Create);
                    stream.Close();
                }
                using (Stream stream = Storage.OpenFile(saveFile, OpenFileMode.ReadWrite))
                {
                    using (var streamSave = new MemoryStream())
                    {
                        var binaryWriter = new BinaryWriter(streamSave);
                        binaryWriter.Write(chunk.Cells.Length);
                        for (int i = 0; i < chunk.Cells.Length; i++)
                        {
                            binaryWriter.Write(chunk.Cells[i]);
                        }
                        binaryWriter.Write(chunk.Shafts.Length);
                        for (int i = 0; i < chunk.Shafts.Length; i++)
                        {
                            binaryWriter.Write(chunk.Shafts[i]);
                        }
                        ModsManager.StreamCompress(stream, streamSave);
                    }
                }


            }
            catch (Exception e) {
                if(fileExists)Storage.DeleteFile(saveFile);
            }
        }

    }
}
