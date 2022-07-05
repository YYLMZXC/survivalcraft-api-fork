using Engine;
using System;
using System.Collections.Generic;
using Engine.Graphics;
namespace Game
{
	public class TerrainChunk : IDisposable
	{
		public struct BrushPaint
		{
			public Point3 Position;

			public TerrainBrush Brush;
		}

		public const int SizeBits = 4;

		public const int Size = 16;

		public const int HeightBits = 8;

		public const int Height = 256;

		public const int SizeMinusOne = 15;

		public const int HeightMinusOne = 255;

		public const int SliceHeight = 16;

		public const int SlicesCount = 16;

		public Terrain Terrain;

		public Point2 Coords;

		public Point2 Origin;

		public BoundingBox BoundingBox;

		public Vector2 Center;

		public TerrainChunkState State;

		public TerrainChunkState ThreadState;

		public bool WasDowngraded;

		public TerrainChunkState? DowngradedState;

		public bool WasUpgraded;

		public TerrainChunkState? UpgradedState;

		public float DrawDistanceSquared;

		public int LightPropagationMask;

		public int ModificationCounter;

		public float[] FogEnds = new float[4];

		public int[] SliceContentsHashes = new int[16];

		public int[] GeneratedSliceContentsHashes = new int[16];

		public bool AreBehaviorsNotified;

		public object lockobj = new object();

		public bool IsLoaded;

		public volatile bool NewGeometryData;

		public TerrainChunkGeometry Geometry = new TerrainChunkGeometry();

		public int[] Cells = new int[65536];

		public int[] Shafts = new int[256];

		public Dictionary<Texture2D, TerrainGeometry[]> Draws = new Dictionary<Texture2D, TerrainGeometry[]>();

		public DynamicArray<TerrainChunkGeometry.Buffer> Buffers = new DynamicArray<TerrainChunkGeometry.Buffer>();

		public DynamicArray<BrushPaint> m_brushPaints = new DynamicArray<BrushPaint>();

		public TerrainChunk(Terrain terrain, int x, int z)
		{
			Terrain = terrain;
			Coords = new Point2(x, z);
			Origin = new Point2(x * 16, z * 16);
			BoundingBox = new BoundingBox(new Vector3(Origin.X, 0f, Origin.Y), new Vector3(Origin.X + 16, 256f, Origin.Y + 16));
			Center = new Vector2((float)Origin.X + 8f, (float)Origin.Y + 8f);
		}
		public void InvalidateSliceContentsHashes()
		{
			for (int i = 0; i < GeneratedSliceContentsHashes.Length; i++)
			{
				GeneratedSliceContentsHashes[i] = 0;
			}
		}
		public void CopySliceContentsHashes() {
			for (int i = 0; i < GeneratedSliceContentsHashes.Length; i++)
			{
				GeneratedSliceContentsHashes[i] = SliceContentsHashes[i];
			}

		}
		public void Dispose()
		{
			Geometry.Dispose();
		}

		public static bool IsCellValid(int x, int y, int z)
		{
			if (x >= 0 && x < 16 && y >= 0 && y < 256 && z >= 0)
			{
				return z < 16;
			}
			return false;
		}

		public static bool IsShaftValid(int x, int z)
		{
			if (x >= 0 && x < 16 && z >= 0)
			{
				return z < 16;
			}
			return false;
		}

		public static int CalculateCellIndex(int x, int y, int z)
		{
			return y + x * 256 + z * 256 * 16;
		}

		public int CalculateTopmostCellHeight(int x, int z)
		{
			int num = CalculateCellIndex(x, 255, z);
			int num2 = 255;
			while (num2 >= 0)
			{
				if (Terrain.ExtractContents(GetCellValueFast(num)) != 0)
				{
					return num2;
				}
				num2--;
				num--;
			}
			return 0;
		}

		public int GetCellValueFast(int index)
		{
			return Cells[index];
		}

		public int GetCellValueFast(int x, int y, int z)
		{
			return Cells[y + x * 256 + z * 256 * 16];
		}

		public void SetCellValueFast(int x, int y, int z, int value)
		{
			Cells[y + x * 256 + z * 256 * 16] = value;
		}

		public void SetCellValueFast(int index, int value)
		{
			Cells[index] = value;
		}

		public int GetCellContentsFast(int x, int y, int z)
		{
			return Terrain.ExtractContents(GetCellValueFast(x, y, z));
		}

		public int GetCellLightFast(int x, int y, int z)
		{
			return Terrain.ExtractLight(GetCellValueFast(x, y, z));
		}

		public int GetShaftValueFast(int x, int z)
		{
			return Shafts[x + z * 16];
		}

		public void SetShaftValueFast(int x, int z, int value)
		{
			Shafts[x + z * 16] = value;
		}

		public int GetTemperatureFast(int x, int z)
		{
			return Terrain.ExtractTemperature(GetShaftValueFast(x, z));
		}

		public void SetTemperatureFast(int x, int z, int temperature)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceTemperature(GetShaftValueFast(x, z), temperature));
		}

		public int GetHumidityFast(int x, int z)
		{
			return Terrain.ExtractHumidity(GetShaftValueFast(x, z));
		}

		public void SetHumidityFast(int x, int z, int humidity)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceHumidity(GetShaftValueFast(x, z), humidity));
		}

		public int GetTopHeightFast(int x, int z)
		{
			return Terrain.ExtractTopHeight(GetShaftValueFast(x, z));
		}

		public void SetTopHeightFast(int x, int z, int topHeight)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceTopHeight(GetShaftValueFast(x, z), topHeight));
		}

		public int GetBottomHeightFast(int x, int z)
		{
			return Terrain.ExtractBottomHeight(GetShaftValueFast(x, z));
		}

		public void SetBottomHeightFast(int x, int z, int bottomHeight)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceBottomHeight(GetShaftValueFast(x, z), bottomHeight));
		}

		public int GetSunlightHeightFast(int x, int z)
		{
			return Terrain.ExtractSunlightHeight(GetShaftValueFast(x, z));
		}

		public void SetSunlightHeightFast(int x, int z, int sunlightHeight)
		{
			SetShaftValueFast(x, z, Terrain.ReplaceSunlightHeight(GetShaftValueFast(x, z), sunlightHeight));
		}

		public void AddBrushPaint(int x, int y, int z, TerrainBrush brush)
		{
			m_brushPaints.Add(new BrushPaint
			{
				Position = new Point3(x, y, z),
				Brush = brush
			});
		}

		public void ApplyBrushPaints(TerrainChunk chunk)
		{
			foreach (BrushPaint brushPaint in m_brushPaints)
			{
				brushPaint.Brush.PaintFast(chunk, brushPaint.Position.X, brushPaint.Position.Y, brushPaint.Position.Z);
			}
		}
	}
}
