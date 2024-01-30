using Engine;
using Engine.Graphics;
using Engine.Media;
using Engine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game
{
	public static class BlocksManager
	{
		public struct ImageExtrusionKey
		{
			public Image Image;

			public int Slot;

			public override int GetHashCode()
			{
				return Image.GetHashCode() ^ Slot.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj != null)
				{
					ImageExtrusionKey imageExtrusionKey = (ImageExtrusionKey)obj;
					if (imageExtrusionKey.Image == Image)
					{
						return imageExtrusionKey.Slot == Slot;
					}
					return false;
				}
				return false;
			}
		}

		public static Block[] m_blocks = new Block[1024];

		public static FluidBlock[] m_fluidBlocks = new FluidBlock[1024];

		public static List<string> m_categories = [];

		public static DrawBlockEnvironmentData m_defaultEnvironmentData = new();

		public static Vector4[] m_slotTexCoords = new Vector4[256];

		public static Dictionary<ImageExtrusionKey, BlockMesh> m_imageExtrusionsCache = [];

		public static Block[] Blocks => m_blocks;

		public static FluidBlock[] FluidBlocks => m_fluidBlocks;

		public static ReadOnlyList<string> Categories => new(m_categories);

		public static bool DrawImageExtrusionEnabled = true;

		public static void Initialize()
		{
			for (int i = 0; i < m_blocks.Length; i++)
			{
				m_blocks[i] = null;
			}
			m_categories.Clear();
			m_categories.Add("Terrain");
			m_categories.Add("Plants");
			m_categories.Add("Construction");
			m_categories.Add("Items");
			m_categories.Add("Tools");
			m_categories.Add("Weapons");
			m_categories.Add("Clothes");
			m_categories.Add("Electrics");
			m_categories.Add("Food");
			m_categories.Add("Spawner Eggs");
			m_categories.Add("Painted");
			m_categories.Add("Dyed");
			m_categories.Add("Fireworks");
			CalculateSlotTexCoordTables();
			int num = 0;
			foreach (ModEntity entity in ModsManager.ModList)
			{
				for (int i = 0; i < entity.Blocks.Count; i++)
				{
					Block block = entity.Blocks[i];
					m_blocks[block.BlockIndex] = block;
					if (block is FluidBlock)
					{
						m_fluidBlocks[block.BlockIndex] = block as FluidBlock;
					}
				}
			}
			for (num = 0; num < m_blocks.Length; num++)
			{
				if (m_blocks[num] == null)
				{
					m_blocks[num] = Blocks[0];
				}
			}
			foreach (ModEntity modEntity in ModsManager.ModList)
			{
				modEntity.LoadBlocksData();
			}
			for (int j = 0; j < m_blocks.Length; j++)
			{
				Block block = m_blocks[j];
				try
				{
					block.Initialize();
				}
				catch (Exception e)
				{
					LoadingScreen.Warning("Loading Block " + block.GetType().FullName + " error." + e.Message);
				}
				foreach (int value in block.GetCreativeValues())
				{
					string category = block.GetCategory(value);
					AddCategory(category);
				}
			}
			GameManager.ProjectDisposed += delegate
			{
				m_imageExtrusionsCache.Clear();
			};
			
			ModInterfacesManager.InvokeHooks("BlocksInitialized", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
			{
				modInterface.BlocksInitialized();
				isContinueRequired = true;
			});
		}

		public static void AddCategory(string category)
		{
			if (!m_categories.Contains(category))
			{
				m_categories.Add(category);
			}
		}

		public static Block? FindBlockByTypeName(string typeName, bool throwIfNotFound)
		{
			var block = Blocks.FirstOrDefault(block => block.GetType().Name == typeName);
			if (block == null && throwIfNotFound)
			{
				throw new InvalidOperationException(string.Format(LanguageControl.Get("BlocksManager", 1), typeName));
			}
			return block;
		}

		public static Block[] FindBlocksByCraftingId(string craftingId)
		{
			List<Block> blocks = [];
			foreach (var c in BlocksManager.Blocks)
			{
				if (c.MatchCrafingId(craftingId)) blocks.Add(c);
			}
			return blocks.ToArray();
		}

		public static void DrawCubeBlock(PrimitivesRenderer3D primitivesRenderer, int value, Vector3 size, ref Matrix matrix, Color color, Color topColor, DrawBlockEnvironmentData environmentData)
		{
			DrawCubeBlock(primitivesRenderer, value, size, ref matrix, color, topColor, environmentData, (environmentData.SubsystemTerrain != null) ? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture : BlocksTexturesManager.DefaultBlocksTexture);
		}

		public static void DrawCubeBlock(PrimitivesRenderer3D primitivesRenderer, int value, Vector3 size, ref Matrix matrix, Color color, Color topColor, DrawBlockEnvironmentData environmentData, Texture2D texture)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			TexturedBatch3D texturedBatch3D = primitivesRenderer.TexturedBatch(texture, useAlphaTest: true, 0, null, RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
			float s = LightingManager.LightIntensityByLightValue[environmentData.Light];
			color = Color.MultiplyColorOnly(color, s);
			topColor = Color.MultiplyColorOnly(topColor, s);
			Vector3 translation = matrix.Translation;
			Vector3 vector = matrix.Right * size.X;
			Vector3 v = matrix.Up * size.Y;
			Vector3 v2 = matrix.Forward * size.Z;
			Vector3 v3 = translation + (0.5f * (-vector - v - v2));
			Vector3 v4 = translation + (0.5f * (vector - v - v2));
			Vector3 v5 = translation + (0.5f * (-vector + v - v2));
			Vector3 v6 = translation + (0.5f * (vector + v - v2));
			Vector3 v7 = translation + (0.5f * (-vector - v + v2));
			Vector3 v8 = translation + (0.5f * (vector - v + v2));
			Vector3 v9 = translation + (0.5f * (-vector + v + v2));
			Vector3 v10 = translation + (0.5f * (vector + v + v2));
			if (environmentData.ViewProjectionMatrix.HasValue)
			{
				Matrix m = environmentData.ViewProjectionMatrix.Value;
				Vector3.Transform(ref v3, ref m, out v3);
				Vector3.Transform(ref v4, ref m, out v4);
				Vector3.Transform(ref v5, ref m, out v5);
				Vector3.Transform(ref v6, ref m, out v6);
				Vector3.Transform(ref v7, ref m, out v7);
				Vector3.Transform(ref v8, ref m, out v8);
				Vector3.Transform(ref v9, ref m, out v9);
				Vector3.Transform(ref v10, ref m, out v10);
			}
			int num = Terrain.ExtractContents(value);
			Block block = Blocks[num];
			Vector4 vector2 = Vector4.Zero;
			int textureSlotCount = block.GetTextureSlotCount(value);
			int textureSlot = block.GetFaceTextureSlot(0, value);
			vector2.X = ((float)(textureSlot % textureSlotCount)) / textureSlotCount;
			vector2.Y = ((float)(textureSlot / textureSlotCount)) / textureSlotCount;
			vector2.W = vector2.Y + (1f / textureSlotCount);
			vector2.Z = vector2.X + (1f / textureSlotCount);
			texturedBatch3D.QueueQuad(color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Forward)), p1: v3, p2: v5, p3: v6, p4: v4, texCoord1: new Vector2(vector2.X, vector2.W), texCoord2: new Vector2(vector2.X, vector2.Y), texCoord3: new Vector2(vector2.Z, vector2.Y), texCoord4: new Vector2(vector2.Z, vector2.W));
			textureSlot = block.GetFaceTextureSlot(2, value);
			vector2.X = ((float)(textureSlot % textureSlotCount)) / textureSlotCount;
			vector2.Y = ((float)(textureSlot / textureSlotCount)) / textureSlotCount;
			vector2.W = vector2.Y + (1f / textureSlotCount);
			vector2.Z = vector2.X + (1f / textureSlotCount);
			texturedBatch3D.QueueQuad(color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(matrix.Forward)), p1: v7, p2: v8, p3: v10, p4: v9, texCoord1: new Vector2(vector2.Z, vector2.W), texCoord2: new Vector2(vector2.X, vector2.W), texCoord3: new Vector2(vector2.X, vector2.Y), texCoord4: new Vector2(vector2.Z, vector2.Y));
			textureSlot = block.GetFaceTextureSlot(5, value);
			vector2.X = ((float)(textureSlot % textureSlotCount)) / textureSlotCount;
			vector2.Y = ((float)(textureSlot / textureSlotCount)) / textureSlotCount;
			vector2.W = vector2.Y + (1f / textureSlotCount);
			vector2.Z = vector2.X + (1f / textureSlotCount);
			texturedBatch3D.QueueQuad(color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Up)), p1: v3, p2: v4, p3: v8, p4: v7, texCoord1: new Vector2(vector2.X, vector2.Y), texCoord2: new Vector2(vector2.Z, vector2.Y), texCoord3: new Vector2(vector2.Z, vector2.W), texCoord4: new Vector2(vector2.X, vector2.W));
			textureSlot = block.GetFaceTextureSlot(4, value);
			vector2.X = ((float)(textureSlot % textureSlotCount)) / textureSlotCount;
			vector2.Y = ((float)(textureSlot / textureSlotCount)) / textureSlotCount;
			vector2.W = vector2.Y + (1f / textureSlotCount);
			vector2.Z = vector2.X + (1f / textureSlotCount);
			texturedBatch3D.QueueQuad(color: Color.MultiplyColorOnly(topColor, LightingManager.CalculateLighting(matrix.Up)), p1: v5, p2: v9, p3: v10, p4: v6, texCoord1: new Vector2(vector2.X, vector2.W), texCoord2: new Vector2(vector2.X, vector2.Y), texCoord3: new Vector2(vector2.Z, vector2.Y), texCoord4: new Vector2(vector2.Z, vector2.W));
			textureSlot = block.GetFaceTextureSlot(1, value);
			vector2.X = ((float)(textureSlot % textureSlotCount)) / textureSlotCount;
			vector2.Y = ((float)(textureSlot / textureSlotCount)) / textureSlotCount;
			vector2.W = vector2.Y + (1f / textureSlotCount);
			vector2.Z = vector2.X + (1f / textureSlotCount);
			texturedBatch3D.QueueQuad(color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Right)), p1: v3, p2: v7, p3: v9, p4: v5, texCoord1: new Vector2(vector2.Z, vector2.W), texCoord2: new Vector2(vector2.X, vector2.W), texCoord3: new Vector2(vector2.X, vector2.Y), texCoord4: new Vector2(vector2.Z, vector2.Y));
			textureSlot = block.GetFaceTextureSlot(3, value);
			vector2.X = ((float)(textureSlot % textureSlotCount)) / textureSlotCount;
			vector2.Y = ((float)(textureSlot / textureSlotCount)) / textureSlotCount;
			vector2.W = vector2.Y + (1f / textureSlotCount);
			vector2.Z = vector2.X + (1f / textureSlotCount);
			texturedBatch3D.QueueQuad(color: Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(matrix.Right)), p1: v4, p2: v6, p3: v10, p4: v8, texCoord1: new Vector2(vector2.X, vector2.W), texCoord2: new Vector2(vector2.X, vector2.Y), texCoord3: new Vector2(vector2.Z, vector2.Y), texCoord4: new Vector2(vector2.Z, vector2.W));
		}

		public static void DrawFlatOrImageExtrusionBlock(PrimitivesRenderer3D primitivesRenderer, int value, float size, ref Matrix matrix, Texture2D texture, Color color, bool isEmissive, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			if (DrawImageExtrusionEnabled && texture == null && !isEmissive && (environmentData.DrawBlockMode == DrawBlockMode.FirstPerson || environmentData.DrawBlockMode == DrawBlockMode.ThirdPerson))
			{
				DrawImageExtrusionBlock(primitivesRenderer, value, size, ref matrix, color, environmentData);
			}
			else
			{
				DrawFlatBlock(primitivesRenderer, value, size, ref matrix, texture, color, isEmissive, environmentData);
			}
		}

		public static void DrawFlatBlock(PrimitivesRenderer3D primitivesRenderer, int value, float size, ref Matrix matrix, Texture2D texture, Color color, bool isEmissive, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			int num = Terrain.ExtractContents(value);
			Block block = Blocks[num];
			Vector4 vector;
			if (texture == null)
			{
				texture = (environmentData.SubsystemTerrain != null) ? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture : BlocksTexturesManager.DefaultBlocksTexture;
			}
			int textureSlotCount = block.GetTextureSlotCount(value);
			int textureSlot = block.GetFaceTextureSlot(-1, value);
			if (textureSlotCount == 16)
			{
				vector = m_slotTexCoords[textureSlot];
			}
			else
			{
				float tx = (float)(textureSlot % textureSlotCount) / textureSlotCount;
				float ty = (float)(textureSlot / textureSlotCount) / textureSlotCount;
				vector = new Vector4(tx, ty, tx + (1f / textureSlotCount), ty + (1f / textureSlotCount));
			}
			if (!isEmissive)
			{
				float s = LightingManager.LightIntensityByLightValue[environmentData.Light];
				color = Color.MultiplyColorOnly(color, s);
			}
			Vector3 translation = matrix.Translation;
			Vector3 vector2;
			Vector3 vector3;
			if (environmentData.BillboardDirection.HasValue)
			{
				vector2 = Vector3.Normalize(Vector3.Cross(environmentData.BillboardDirection.Value, Vector3.UnitY));
				vector3 = -Vector3.Normalize(Vector3.Cross(environmentData.BillboardDirection.Value, vector2));
			}
			else
			{
				vector2 = matrix.Right;
				vector3 = matrix.Up;
			}
			Vector3 v = translation + (0.85f * size * (-vector2 - vector3));
			Vector3 v2 = translation + (0.85f * size * (vector2 - vector3));
			Vector3 v3 = translation + (0.85f * size * (-vector2 + vector3));
			Vector3 v4 = translation + (0.85f * size * (vector2 + vector3));
			if (environmentData.ViewProjectionMatrix.HasValue)
			{
				Matrix m = environmentData.ViewProjectionMatrix.Value;
				Vector3.Transform(ref v, ref m, out v);
				Vector3.Transform(ref v2, ref m, out v2);
				Vector3.Transform(ref v3, ref m, out v3);
				Vector3.Transform(ref v4, ref m, out v4);
			}
			TexturedBatch3D texturedBatch3D = primitivesRenderer.TexturedBatch(texture, useAlphaTest: true, 0, null, RasterizerState.CullCounterClockwiseScissor, BlendState.AlphaBlend, SamplerState.PointClamp);
			texturedBatch3D.QueueQuad(v, v3, v4, v2, new Vector2(vector.X, vector.W), new Vector2(vector.X, vector.Y), new Vector2(vector.Z, vector.Y), new Vector2(vector.Z, vector.W), color);
			if (!environmentData.BillboardDirection.HasValue)
			{
				texturedBatch3D.QueueQuad(v, v2, v4, v3, new Vector2(vector.X, vector.W), new Vector2(vector.Z, vector.W), new Vector2(vector.Z, vector.Y), new Vector2(vector.X, vector.Y), color);
			}
		}

		public static void DrawImageExtrusionBlock(PrimitivesRenderer3D primitivesRenderer, int value, float size, ref Matrix matrix, Color color, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			int num = Terrain.ExtractContents(value);
			Block block = Blocks[num];
			try
			{
				Image image;
				if (environmentData.SubsystemTerrain != null)
				{
					image = (Image)environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture.Tag;
				}
				else
				{
					image = (Image)BlocksTexturesManager.DefaultBlocksTexture.Tag;
				}
				BlockMesh imageExtrusionBlockMesh = GetImageExtrusionBlockMesh(image, block.GetFaceTextureSlot(-1, value));
				DrawMeshBlock(primitivesRenderer, imageExtrusionBlockMesh, color, 1.7f * size, ref matrix, environmentData);
			}
			catch (Exception)
			{
			}
		}

		public static BlockMesh GetImageExtrusionBlockMesh(Image image, int slot)
		{
			ImageExtrusionKey imageExtrusionKey = default(ImageExtrusionKey);
			imageExtrusionKey.Image = image;
			imageExtrusionKey.Slot = slot;
			ImageExtrusionKey key = imageExtrusionKey;
			if (!m_imageExtrusionsCache.TryGetValue(key, out var value))
			{
				value = new BlockMesh();
				int num = (int)MathUtils.Round(m_slotTexCoords[slot].X * (float)image.Width);
				int num2 = (int)MathUtils.Round(m_slotTexCoords[slot].Y * (float)image.Height);
				int num3 = (int)MathUtils.Round(m_slotTexCoords[slot].Z * (float)image.Width);
				int num4 = (int)MathUtils.Round(m_slotTexCoords[slot].W * (float)image.Height);
				int num5 = MathUtils.Max(num3 - num, num4 - num2);
				value.AppendImageExtrusion(image, new Rectangle(num, num2, num3 - num, num4 - num2), new Vector3(1f / (float)num5, 1f / (float)num5, 0.0833333358f), Color.White, 0);
				m_imageExtrusionsCache.Add(key, value);
			}
			return value;
		}

		public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			Texture2D texture = (environmentData.SubsystemTerrain != null) ? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture : BlocksTexturesManager.DefaultBlocksTexture;
			DrawMeshBlock(primitivesRenderer, blockMesh, texture, Color.White, size, ref matrix, environmentData);
		}

		public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			Texture2D texture = (environmentData.SubsystemTerrain != null) ? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture : BlocksTexturesManager.DefaultBlocksTexture;
			DrawMeshBlock(primitivesRenderer, blockMesh, texture, color, size, ref matrix, environmentData);
		}

		public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, Texture2D texture, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			float num = LightingManager.LightIntensityByLightValue[environmentData.Light];
			var v = new Vector4(color);
			v.X *= num;
			v.Y *= num;
			v.Z *= num;
			bool flag = v == Vector4.One;
			TexturedBatch3D texturedBatch3D = primitivesRenderer.TexturedBatch(texture, useAlphaTest: true, 0, null, RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
			bool flag2 = false;
			Matrix m = (!environmentData.ViewProjectionMatrix.HasValue) ? matrix : (matrix * environmentData.ViewProjectionMatrix.Value);
			if (size != 1f)
			{
				m = Matrix.CreateScale(size) * m;
			}
			if (m.M14 != 0f || m.M24 != 0f || m.M34 != 0f || m.M44 != 1f)
			{
				flag2 = true;
			}
			int count = blockMesh.Vertices.Count;
			BlockMeshVertex[] array = blockMesh.Vertices.Array;
			int count2 = blockMesh.Indices.Count;
			int[] array2 = blockMesh.Indices.Array;
			DynamicArray<VertexPositionColorTexture> triangleVertices = texturedBatch3D.TriangleVertices;
			int count3 = triangleVertices.Count;
			int count4 = triangleVertices.Count;
			triangleVertices.Count += count;
			for (int i = 0; i < count; i++)
			{
				BlockMeshVertex blockMeshVertex = array[i];
				if (flag2)
				{
					var v2 = new Vector4(blockMeshVertex.Position, 1f);
					Vector4.Transform(ref v2, ref m, out v2);
					float num2 = 1f / v2.W;
					blockMeshVertex.Position = new Vector3(v2.X * num2, v2.Y * num2, v2.Z * num2);
				}
				else
				{
					Vector3.Transform(ref blockMeshVertex.Position, ref m, out blockMeshVertex.Position);
				}
				if (flag || blockMeshVertex.IsEmissive)
				{
					triangleVertices.Array[count4++] = new VertexPositionColorTexture(blockMeshVertex.Position, blockMeshVertex.Color, blockMeshVertex.TextureCoordinates);
					continue;
				}
				var color2 = new Color((byte)(blockMeshVertex.Color.R * v.X), (byte)(blockMeshVertex.Color.G * v.Y), (byte)(blockMeshVertex.Color.B * v.Z), (byte)(blockMeshVertex.Color.A * v.W));
				triangleVertices.Array[count4++] = new VertexPositionColorTexture(blockMeshVertex.Position, color2, blockMeshVertex.TextureCoordinates);
			}
			DynamicArray<int> triangleIndices = texturedBatch3D.TriangleIndices;
			int count5 = triangleIndices.Count;
			triangleIndices.Count += count2;
			for (int j = 0; j < count2; j++)
			{
				triangleIndices.Array[count5++] = count3 + array2[j];
			}
		}

		public static int DamageItem(int value, int damageCount)
		{
			int num = Terrain.ExtractContents(value);
			Block block = Blocks[num];
			if (block.Durability >= 0)
			{
				int num2 = block.GetDamage(value) + damageCount;
				if (num2 <= block.Durability)
				{
					return block.SetDamage(value, num2);
				}
				return block.GetDamageDestructionValue(value);
			}
			return value;
		}

		public static void LoadBlocksData(string data)
		{
			var dictionary = new Dictionary<Block, bool>();
			data = data.Replace("\r", string.Empty);
			string[] array = data.Split(new char[1]
			{
				'\n'
			}, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = null;
			for (int i = 0; i < array.Length; i++)
			{
				if (string.IsNullOrEmpty(array[i])) continue;
				string[] array3 = array[i].Split(';');
				if (i == 0)
				{
					array2 = new string[array3.Length - 1];
					Array.Copy(array3, 1, array2, 0, array3.Length - 1);
					continue;
				}
				if (array3.Length != array2.Length + 1)
				{
					throw new InvalidOperationException(string.Format(LanguageControl.Get("BlocksManager", 2), (array3.Length != 0) ? array3[0] : LanguageControl.Unknown));
				}
				string typeName = array3[0];
				if (string.IsNullOrEmpty(typeName))
				{
					continue;
				}
				Block block = m_blocks.FirstOrDefault((Block v) => v.GetType().Name == typeName);
				if (block == null)
				{
					throw new InvalidOperationException(string.Format(LanguageControl.Get("BlocksManager", 3), typeName));
				}
				dictionary.Add(block, value: true);
				var dictionary2 = new Dictionary<string, FieldInfo>();
				foreach (FieldInfo runtimeField in block.GetType().GetRuntimeFields())
				{
					if (runtimeField.IsPublic && !runtimeField.IsStatic)
					{
						dictionary2.Add(runtimeField.Name, runtimeField);
					}
				}
				for (int j = 1; j < array3.Length; j++)
				{
					string text = array2[j - 1];
					string text2 = array3[j];
					if (!string.IsNullOrEmpty(text2))
					{
						if (!dictionary2.TryGetValue(text, out FieldInfo value))
						{
							throw new InvalidOperationException(string.Format(LanguageControl.Get("BlocksManager", 5), text));
						}
						object obj = null;
						if (text2.StartsWith("#"))
						{
							string refTypeName = text2.Substring(1);
							obj = (!string.IsNullOrEmpty(refTypeName)) ? (m_blocks.FirstOrDefault((Block v) => v.GetType().Name == refTypeName) ?? throw new InvalidOperationException(string.Format(LanguageControl.Get("BlocksManager", 6), refTypeName))).BlockIndex : ((object)block.BlockIndex);
						}
						else
						{
							obj = HumanReadableConverter.ConvertFromString(value.FieldType, text2);
						}
						value.SetValue(block, obj);
					}
				}
			}
		}

		public static void CalculateSlotTexCoordTables()
		{
			for (int i = 0; i < 256; i++)
			{
				m_slotTexCoords[i] = TextureSlotToTextureCoords(i);
			}
		}

		public static Vector4 TextureSlotToTextureCoords(int slot)
		{
			int num = slot % 16;
			int num2 = slot / 16;
			float x = (num + 0.001f) / 16f;
			float y = (num2 + 0.001f) / 16f;
			float z = (num + 1 - 0.001f) / 16f;
			float w = (num2 + 1 - 0.001f) / 16f;
			return new Vector4(x, y, z, w);
		}

		public static Vector4[] GetslotTexCoords(int textureSlotCount)
		{
			int totalCount = textureSlotCount * textureSlotCount;
			Vector4[] slotTexCoords = new Vector4[totalCount];
			for (int i = 0; i < totalCount; i++)
			{
				int num = i % textureSlotCount;
				int num2 = i / textureSlotCount;
				float x = (num + 0.001f) / (float)textureSlotCount;
				float y = (num2 + 0.001f) / (float)textureSlotCount;
				float z = (num + 1 - 0.001f) / (float)textureSlotCount;
				float w = (num2 + 1 - 0.001f) / (float)textureSlotCount;
				slotTexCoords[i] = new Vector4(x, y, z, w);
			}
			return slotTexCoords;
		}

		public static Block GetBlock(string ModSpace, string TypeFullName)
		{
			if (ModsManager.GetModEntity(ModSpace, out ModEntity modEntity))
			{
				Block block = modEntity.Blocks.Find(p => p.GetType().Name == TypeFullName);
				if (block != null)
				{
					return block;
				}
			}
			return null;
		}

	}
}
