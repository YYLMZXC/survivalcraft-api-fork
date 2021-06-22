using Engine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game
{
    public class TerrainSerializer22 : IDisposable
    {
        public const int MaxChunks = 65536;

        public const int TocEntryBytesCount = 12;

        public const int TocBytesCount = 786444;

        public const int ChunkSizeX = 16;

        public const int ChunkSizeY = 256;

        public const int ChunkSizeZ = 16;

        public const int ChunkBitsX = 4;

        public const int ChunkBitsZ = 4;

        public const int ChunkBytesCount = 263184;

        public Terrain m_terrain;

        public Stream m_stream;

        public string DirectoryName;

        public TerrainSerializer22(Terrain terrain, string directoryName)
        {
            m_terrain = terrain;
            directoryName = Storage.CombinePaths(directoryName,"Chunks");
            if (!Storage.DirectoryExists(directoryName)) {
                Storage.CreateDirectory(directoryName);
            }
            this.DirectoryName = directoryName;

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
            Utilities.Dispose(ref m_stream);
        }
        /// <summary>
        /// 把Stream读为Chunk
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool LoadChunkBlocks(TerrainChunk chunk)
        {
            string readFile = Storage.CombinePaths(DirectoryName, $"{(uint)chunk.Coords.GetHashCode()}.dat");
            if (Storage.FileExists(readFile))
            {
                using (Stream stream = Storage.OpenFile(readFile, OpenFileMode.CreateOrOpen))
                {
                    using (Stream memory = ModsManager.StreamDecompress(stream)) {
                        BinaryReader binaryReader = new BinaryReader(memory);
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
        /// <summary>
        /// 把Chunk储存为Stream
        /// </summary>
        /// <param name="chunk"></param>
        public void SaveChunkBlocks(TerrainChunk chunk)
        {
            string saveFile = Storage.CombinePaths(DirectoryName, $"{(uint)chunk.Coords.GetHashCode()}.dat");
            if (!Storage.FileExists(saveFile)) {
                Stream stream = Storage.OpenFile(saveFile,OpenFileMode.Create);
                stream.Close();
            }
            using (Stream stream = Storage.OpenFile(saveFile, OpenFileMode.ReadWrite))
            {
                using (MemoryStream streamSave = new MemoryStream()) {
                    BinaryWriter binaryWriter = new BinaryWriter(streamSave);
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
                    ModsManager.StreamCompress(stream,streamSave);
                }
            }
        }

    }
}
