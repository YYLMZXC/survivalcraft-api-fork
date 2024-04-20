using Engine;
using Engine.Graphics;
using Engine.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Game
{
	public class BlockMesh
	{
		public struct InternalVertex
		{
			public Vector3 Position;

			public Vector3 Normal;

			public Vector2 TextureCoordinate;
		}

		public DynamicArray<BlockMeshVertex> Vertices = [];

		public DynamicArray<int> Indices = [];

		public DynamicArray<sbyte> Sides;

		public BoundingBox CalculateBoundingBox()
		{
			return new BoundingBox(Vertices.Select((BlockMeshVertex v) => v.Position));
		}

		public BoundingBox CalculateBoundingBox(Matrix matrix)
		{
			return new BoundingBox(Vertices.Select((BlockMeshVertex v) => Vector3.Transform(v.Position, matrix)));
		}

		public static Matrix GetBoneAbsoluteTransform(ModelBone modelBone)
		{
			if (modelBone.ParentBone != null)
			{
				return GetBoneAbsoluteTransform(modelBone.ParentBone) * modelBone.Transform;
			}
			return modelBone.Transform;
		}

		public void AppendImageExtrusion(Image image, Rectangle bounds, Vector3 size, Color color)
		{
			var blockMesh = new BlockMesh();
			DynamicArray<BlockMeshVertex> vertices = blockMesh.Vertices;
			DynamicArray<int> indices = blockMesh.Indices;
			var item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Left, bounds.Top, -1f),
				TextureCoordinates = new Vector2(bounds.Left, bounds.Top)
			};
			vertices.Add(item);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Right, bounds.Top, -1f),
				TextureCoordinates = new Vector2(bounds.Right, bounds.Top)
			};
			vertices.Add(item);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Left, bounds.Bottom, -1f),
				TextureCoordinates = new Vector2(bounds.Left, bounds.Bottom)
			};
			vertices.Add(item);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Right, bounds.Bottom, -1f),
				TextureCoordinates = new Vector2(bounds.Right, bounds.Bottom)
			};
			vertices.Add(item);
			indices.Add(vertices.Count - 4);
			indices.Add(vertices.Count - 1);
			indices.Add(vertices.Count - 3);
			indices.Add(vertices.Count - 1);
			indices.Add(vertices.Count - 4);
			indices.Add(vertices.Count - 2);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Left, bounds.Top, 1f),
				TextureCoordinates = new Vector2(bounds.Left, bounds.Top)
			};
			vertices.Add(item);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Right, bounds.Top, 1f),
				TextureCoordinates = new Vector2(bounds.Right, bounds.Top)
			};
			vertices.Add(item);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Left, bounds.Bottom, 1f),
				TextureCoordinates = new Vector2(bounds.Left, bounds.Bottom)
			};
			vertices.Add(item);
			item = new BlockMeshVertex
			{
				Position = new Vector3(bounds.Right, bounds.Bottom, 1f),
				TextureCoordinates = new Vector2(bounds.Right, bounds.Bottom)
			};
			vertices.Add(item);
			indices.Add(vertices.Count - 4);
			indices.Add(vertices.Count - 3);
			indices.Add(vertices.Count - 1);
			indices.Add(vertices.Count - 1);
			indices.Add(vertices.Count - 2);
			indices.Add(vertices.Count - 4);
			for (int i = bounds.Left - 1; i <= bounds.Right; i++)
			{
				int num = -1;
				for (int j = bounds.Top - 1; j <= bounds.Bottom; j++)
				{
					bool num2 = !bounds.Contains(new Point2(i, j)) || image.GetPixelFast(i, j).IsCompletelyTransparent();
					bool flag = bounds.Contains(new Point2(i - 1, j)) && !image.GetPixelFast(i - 1, j).IsCompletelyTransparent();
					if (num2 & flag)
					{
						if (num < 0)
						{
							num = j;
						}
					}
					else if (num >= 0)
					{
						item = new BlockMeshVertex
						{
							Position = new Vector3(i - 0.01f, num - 0.01f, -1.01f),
							TextureCoordinates = new Vector2(i - 1 + 0.01f, num + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(i - 0.01f, num - 0.01f, 1.01f),
							TextureCoordinates = new Vector2(i - 0.01f, num + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(i - 0.01f, j + 0.01f, -1.01f),
							TextureCoordinates = new Vector2(i - 1 + 0.01f, j - 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(i - 0.01f, j + 0.01f, 1.01f),
							TextureCoordinates = new Vector2(i - 0.01f, j - 0.01f)
						};
						vertices.Add(item);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 3);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 2);
						num = -1;
					}
				}
			}
			for (int k = bounds.Left - 1; k <= bounds.Right; k++)
			{
				int num3 = -1;
				for (int l = bounds.Top - 1; l <= bounds.Bottom; l++)
				{
					bool num4 = !bounds.Contains(new Point2(k, l)) || image.GetPixelFast(k, l).IsCompletelyTransparent();
					bool flag2 = bounds.Contains(new Point2(k + 1, l)) && !image.GetPixelFast(k + 1, l).IsCompletelyTransparent();
					if (num4 & flag2)
					{
						if (num3 < 0)
						{
							num3 = l;
						}
					}
					else if (num3 >= 0)
					{
						item = new BlockMeshVertex
						{
							Position = new Vector3(k + 1 + 0.01f, num3 - 0.01f, -1.01f),
							TextureCoordinates = new Vector2(k + 1 + 0.01f, num3 + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(k + 1 + 0.01f, num3 - 0.01f, 1.01f),
							TextureCoordinates = new Vector2(k + 2 - 0.01f, num3 + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(k + 1 + 0.01f, l + 0.01f, -1.01f),
							TextureCoordinates = new Vector2(k + 1 + 0.01f, l - 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(k + 1 + 0.01f, l + 0.01f, 1.01f),
							TextureCoordinates = new Vector2(k + 2 - 0.01f, l - 0.01f)
						};
						vertices.Add(item);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 3);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 2);
						indices.Add(vertices.Count - 4);
						num3 = -1;
					}
				}
			}
			for (int m = bounds.Top - 1; m <= bounds.Bottom; m++)
			{
				int num5 = -1;
				for (int n = bounds.Left - 1; n <= bounds.Right; n++)
				{
					bool num6 = !bounds.Contains(new Point2(n, m)) || image.GetPixelFast(n, m).IsCompletelyTransparent();
					bool flag3 = bounds.Contains(new Point2(n, m - 1)) && !image.GetPixelFast(n, m - 1).IsCompletelyTransparent();
					if (num6 & flag3)
					{
						if (num5 < 0)
						{
							num5 = n;
						}
					}
					else if (num5 >= 0)
					{
						item = new BlockMeshVertex
						{
							Position = new Vector3(num5 - 0.01f, m - 0.01f, -1.01f),
							TextureCoordinates = new Vector2(num5 + 0.01f, m - 1 + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(num5 - 0.01f, m - 0.01f, 1.01f),
							TextureCoordinates = new Vector2(num5 + 0.01f, m - 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(n + 0.01f, m - 0.01f, -1.01f),
							TextureCoordinates = new Vector2(n - 0.01f, m - 1 + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(n + 0.01f, m - 0.01f, 1.01f),
							TextureCoordinates = new Vector2(n - 0.01f, m - 0.01f)
						};
						vertices.Add(item);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 3);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 2);
						indices.Add(vertices.Count - 4);
						num5 = -1;
					}
				}
			}
			for (int num7 = bounds.Top - 1; num7 <= bounds.Bottom; num7++)
			{
				int num8 = -1;
				for (int num9 = bounds.Left - 1; num9 <= bounds.Right; num9++)
				{
					bool num10 = !bounds.Contains(new Point2(num9, num7)) || image.GetPixelFast(num9, num7).IsCompletelyTransparent();
					bool flag4 = bounds.Contains(new Point2(num9, num7 + 1)) && !image.GetPixelFast(num9, num7 + 1).IsCompletelyTransparent();
					if (num10 & flag4)
					{
						if (num8 < 0)
						{
							num8 = num9;
						}
					}
					else if (num8 >= 0)
					{
						item = new BlockMeshVertex
						{
							Position = new Vector3(num8 - 0.01f, num7 + 1 + 0.01f, -1.01f),
							TextureCoordinates = new Vector2(num8 + 0.01f, num7 + 1 + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(num8 - 0.01f, num7 + 1 + 0.01f, 1.01f),
							TextureCoordinates = new Vector2(num8 + 0.01f, num7 + 2 - 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(num9 + 0.01f, num7 + 1 + 0.01f, -1.01f),
							TextureCoordinates = new Vector2(num9 - 0.01f, num7 + 1 + 0.01f)
						};
						vertices.Add(item);
						item = new BlockMeshVertex
						{
							Position = new Vector3(num9 + 0.01f, num7 + 1 + 0.01f, 1.01f),
							TextureCoordinates = new Vector2(num9 - 0.01f, num7 + 2 - 0.01f)
						};
						vertices.Add(item);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 3);
						indices.Add(vertices.Count - 1);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 2);
						num8 = -1;
					}
				}
			}
			for (int num11 = 0; num11 < vertices.Count; num11++)
			{
				vertices.Array[num11].Position.X -= bounds.Left + (bounds.Width / 2f);
				vertices.Array[num11].Position.Y = bounds.Bottom - vertices.Array[num11].Position.Y - (bounds.Height / 2f);
				vertices.Array[num11].Position.X *= size.X / bounds.Width;
				vertices.Array[num11].Position.Y *= size.Y / bounds.Height;
				vertices.Array[num11].Position.Z *= size.Z / 2f;
				vertices.Array[num11].TextureCoordinates.X /= image.Width;
				vertices.Array[num11].TextureCoordinates.Y /= image.Height;
				vertices.Array[num11].Color = color;
			}
			AppendBlockMesh(blockMesh);
		}

		public void AppendModelMeshPart(ModelMeshPart meshPart, Matrix matrix, bool makeEmissive, bool flipWindingOrder, bool doubleSided, bool flipNormals, Color color)
		{
			VertexBuffer vertexBuffer = meshPart.VertexBuffer;
			IndexBuffer indexBuffer = meshPart.IndexBuffer;
			ReadOnlyList<VertexElement> vertexElements = vertexBuffer.VertexDeclaration.VertexElements;
			if (vertexElements.Count != 3 || vertexElements[0].Offset != 0 || vertexElements[0].Semantic != VertexElementSemantic.Position.GetSemanticString() || vertexElements[1].Offset != 12 || vertexElements[1].Semantic != VertexElementSemantic.Normal.GetSemanticString() || vertexElements[2].Offset != 24 || vertexElements[2].Semantic != VertexElementSemantic.TextureCoordinate.GetSemanticString())
			{
				throw new InvalidOperationException("Wrong vertex format for a block mesh.");
			}
			InternalVertex[] vertexData = GetVertexData<InternalVertex>(vertexBuffer);
			int[] indexData = GetIndexData<int>(indexBuffer);
			var dictionary = new Dictionary<int, int>();
			for (int i = meshPart.StartIndex; i < meshPart.StartIndex + meshPart.IndicesCount; i++)
			{
				int num = indexData[i];
				if (!dictionary.ContainsKey(num))
				{
					dictionary.Add(num, Vertices.Count);
					BlockMeshVertex item = default;
					item.Position = Vector3.Transform(vertexData[num].Position, matrix);
					item.TextureCoordinates = vertexData[num].TextureCoordinate;
					var vector = Vector3.Normalize(Vector3.TransformNormal(flipNormals ? (-vertexData[num].Normal) : vertexData[num].Normal, matrix));
					if (makeEmissive)
					{
						item.IsEmissive = true;
						item.Color = color;
					}
					else
					{
						item.Color = color * LightingManager.CalculateLighting(vector);
						item.Color.A = color.A;
					}
					item.Face = (byte)CellFace.Vector3ToFace(vector);
					Vertices.Add(item);
				}
			}
			for (int j = 0; j < meshPart.IndicesCount / 3; j++)
			{
				if (doubleSided)
				{
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j)]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 1]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 2]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j)]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 2]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 1]]);
				}
				else if (flipWindingOrder)
				{
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j)]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 2]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 1]]);
				}
				else
				{
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j)]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 1]]);
					Indices.Add(dictionary[indexData[meshPart.StartIndex + (3 * j) + 2]]);
				}
			}
			Trim();
		}

		public void AppendBlockMesh(BlockMesh blockMesh)
		{
			int count = Vertices.Count;
			for (int i = 0; i < blockMesh.Vertices.Count; i++)
			{
				Vertices.Add(blockMesh.Vertices.Array[i]);
			}
			for (int j = 0; j < blockMesh.Indices.Count; j++)
			{
				Indices.Add(blockMesh.Indices.Array[j] + count);
			}
			Trim();
		}

		public void BlendBlockMesh(BlockMesh blockMesh, float factor)
		{
			if (blockMesh.Vertices.Count != Vertices.Count)
			{
				throw new InvalidOperationException("Meshes do not match.");
			}
			for (int i = 0; i < Vertices.Count; i++)
			{
				Vector3 position = Vertices.Array[i].Position;
				Vector3 position2 = blockMesh.Vertices.Array[i].Position;
				Vertices.Array[i].Position = Vector3.Lerp(position, position2, factor);
			}
		}

		public void TransformPositions(Matrix matrix, int facesMask = -1)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (((1 << Vertices.Array[i].Face) & facesMask) != 0)
				{
					Vertices.Array[i].Position = Vector3.Transform(Vertices.Array[i].Position, matrix);
				}
			}
		}

		public void TransformTextureCoordinates(Matrix matrix, int facesMask = -1)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (((1 << Vertices.Array[i].Face) & facesMask) != 0)
				{
					Vertices.Array[i].TextureCoordinates = Vector2.Transform(Vertices.Array[i].TextureCoordinates, matrix);
				}
			}
		}

		public void SetColor(Color color, int facesMask = -1)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (((1 << Vertices.Array[i].Face) & facesMask) != 0)
				{
					Vertices.Array[i].Color = color;
				}
			}
		}

		public void ModulateColor(Color color, int facesMask = -1)
		{
			for (int i = 0; i < Vertices.Count; i++)
			{
				if (((1 << Vertices.Array[i].Face) & facesMask) != 0)
				{
					Vertices.Array[i].Color *= color;
				}
			}
		}

		public void GenerateSidesData()
		{
			Sides = [];
			Sides.Count = Indices.Count / 3;
			for (int i = 0; i < Sides.Count; i++)
			{
				int num = Indices.Array[3 * i];
				int num2 = Indices.Array[(3 * i) + 1];
				int num3 = Indices.Array[(3 * i) + 2];
				Vector3 position = Vertices.Array[num].Position;
				Vector3 position2 = Vertices.Array[num2].Position;
				Vector3 position3 = Vertices.Array[num3].Position;
				if (IsNear(position.Z, position2.Z, position3.Z, 1f))
				{
					Sides.Array[i] = 0;
				}
				else if (IsNear(position.X, position2.X, position3.X, 1f))
				{
					Sides.Array[i] = 1;
				}
				else if (IsNear(position.Z, position2.Z, position3.Z, 0f))
				{
					Sides.Array[i] = 2;
				}
				else if (IsNear(position.X, position2.X, position3.X, 0f))
				{
					Sides.Array[i] = 3;
				}
				else if (IsNear(position.Y, position2.Y, position3.Y, 1f))
				{
					Sides.Array[i] = 4;
				}
				else
				{
					Sides.Array[i] = IsNear(position.Y, position2.Y, position3.Y, 0f) ? (sbyte)5 : (sbyte)-1;
				}
			}
		}

		public void Trim()
		{
			Vertices.Capacity = Vertices.Count;
			Indices.Capacity = Indices.Count;
			if (Sides != null)
			{
				Sides.Capacity = Sides.Count;
			}
		}

		public static T[] GetVertexData<T>(VertexBuffer vertexBuffer)
		{
			byte[] array = vertexBuffer.Tag as byte[];
			if (array == null)
			{
				throw new InvalidOperationException("VertexBuffer does not contain source data in Tag.");
			}
			if (array.Length % Utilities.SizeOf<T>() != 0)
			{
				throw new InvalidOperationException("VertexBuffer data size is not a whole multiply of target type size.");
			}
			var array2 = new T[array.Length / Utilities.SizeOf<T>()];
			var gCHandle = GCHandle.Alloc(array2, GCHandleType.Pinned);
			try
			{
				Marshal.Copy(array, 0, gCHandle.AddrOfPinnedObject(), Utilities.SizeOf<T>() * array2.Length);
				return array2;
			}
			finally
			{
				gCHandle.Free();
			}
		}

		public static T[] GetIndexData<T>(IndexBuffer indexBuffer)
		{
			byte[] array = indexBuffer.Tag as byte[];
			if (array == null)
			{
				throw new InvalidOperationException("IndexBuffer does not contain source data in Tag.");
			}
			if (array.Length % Utilities.SizeOf<T>() != 0)
			{
				throw new InvalidOperationException("IndexBuffer data size is not a whole multiply of target type size.");
			}
			var array2 = new T[array.Length / Utilities.SizeOf<T>()];
			var gCHandle = GCHandle.Alloc(array2, GCHandleType.Pinned);
			try
			{
				Marshal.Copy(array, 0, gCHandle.AddrOfPinnedObject(), Utilities.SizeOf<T>() * array2.Length);
				return array2;
			}
			finally
			{
				gCHandle.Free();
			}
		}

		public static bool IsNear(float v1, float v2, float v3, float t)
		{
			if (v1 - t >= -0.001f && v1 - t <= 0.001f && v2 - t >= -0.001f && v2 - t <= 0.001f && v3 - t >= -0.001f)
			{
				return v3 - t <= 0.001f;
			}
			return false;
		}

		public void AppendImageExtrusion(Image image, Rectangle bounds, Vector3 scale, Color color, int alphaThreshold)
		{
			int count = Vertices.Count;
			AppendImageExtrusionSlice(image, bounds, new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), color, alphaThreshold);
			AppendImageExtrusionSlice(image, bounds, new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f), color, alphaThreshold);
			for (int i = bounds.Left; i < bounds.Right; i++)
			{
				Image image2 = new(1, bounds.Height);
				for (int j = bounds.Top; j < bounds.Bottom; j++)
				{
					if (i == bounds.Left || image.GetPixelFast(i - 1, j).A <= alphaThreshold)
					{
						image2.SetPixelFast(0, j - bounds.Top, image.GetPixelFast(i, j));
					}
				}
				AppendImageExtrusionSlice(image2, new Rectangle(0, 0, image2.Width, image2.Height), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f), new Vector3(i, bounds.Top, 0f), color, alphaThreshold);
			}
			for (int k = bounds.Left; k < bounds.Right; k++)
			{
				Image image3 = new(1, bounds.Height);
				for (int l = bounds.Top; l < bounds.Bottom; l++)
				{
					if (k == bounds.Right - 1 || image.GetPixelFast(k + 1, l).A <= alphaThreshold)
					{
                        image3.SetPixelFast(0, l - bounds.Top, image.GetPixelFast(k, l));
                    }
				}
				AppendImageExtrusionSlice(image3, new Rectangle(0, 0, image3.Width, image3.Height), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(-1f, 0f, 0f), new Vector3(k + 1, bounds.Top, 0f), color, alphaThreshold);
			}
			for (int m = bounds.Top; m < bounds.Bottom; m++)
			{
				Image image4 = new(bounds.Width, 1);
				for (int n = bounds.Left; n < bounds.Right; n++)
				{
					if (m == bounds.Top || image.GetPixelFast(n, m-1).A <= alphaThreshold)
					{
						image4.SetPixelFast(n - bounds.Left, 0, image.GetPixelFast(n, m));
					}
				}
				AppendImageExtrusionSlice(image4, new Rectangle(0, 0, image4.Width, image4.Height), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(bounds.Left, m, 0f), color, alphaThreshold);
			}
			for (int num = bounds.Top; num < bounds.Bottom; num++)
			{
				Image image5 = new(bounds.Width, 1);
				for (int num2 = bounds.Left; num2 < bounds.Right; num2++)
				{
					if (num == bounds.Bottom - 1 || image.GetPixelFast(num2, num +1).A <= alphaThreshold)
					{
						image5.SetPixelFast(num2 - bounds.Left, 0, image.GetPixelFast(num2, num));
					}
				}
				AppendImageExtrusionSlice(image5, new Rectangle(0, 0, image5.Width, image5.Height), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(0f, -1f, 0f), new Vector3(bounds.Left, num + 1, 0f), color, alphaThreshold);
			}
			for (int num3 = count; num3 < Vertices.Count; num3++)
			{
				Vertices.Array[num3].Position.X -= (float)(bounds.Left + bounds.Right) / 2f;
				Vertices.Array[num3].Position.Y -= (float)(bounds.Top + bounds.Bottom) / 2f;
				Vertices.Array[num3].Position.Z -= 0.5f;
				Vertices.Array[num3].Position.X *= scale.X;
				Vertices.Array[num3].Position.Y *= 0f - scale.Y;
				Vertices.Array[num3].Position.Z *= scale.Z;
				Vertices.Array[num3].TextureCoordinates.X /= image.Width;
				Vertices.Array[num3].TextureCoordinates.Y /= image.Height;
				Vertices.Array[num3].Color *= color;
			}
		}

		public void AppendImageExtrusionSlice(Image slice, Rectangle bounds, Vector3 right, Vector3 up, Vector3 forward, Vector3 position, Color color, int alphaThreshold)
		{
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int num3 = int.MinValue;
			int num4 = int.MinValue;
			for (int i = bounds.Top; i < bounds.Bottom; i++)
			{
				for (int j = bounds.Left; j < bounds.Right; j++)
				{
					if (slice.GetPixelFast(j, i).A > alphaThreshold)
					{
						num = MathUtils.Min(num, j);
						num2 = MathUtils.Min(num2, i);
						num3 = MathUtils.Max(num3, j);
						num4 = MathUtils.Max(num4, i);
					}
				}
			}
			if (num != int.MaxValue)
			{
				Matrix m = new(right.X, right.Y, right.Z, 0f, up.X, up.Y, up.Z, 0f, forward.X, forward.Y, forward.Z, 0f, position.X, position.Y, position.Z, 1f);
				bool flip = m.Determinant() > 0f;
				float s = LightingManager.CalculateLighting(-forward);
				Vector3 p = Vector3.Transform(new Vector3(num, num2, 0f), m);
				Vector3 p2 = Vector3.Transform(new Vector3(num3 + 1, num2, 0f), m);
				Vector3 p3 = Vector3.Transform(new Vector3(num, num4 + 1, 0f), m);
				Vector3 p4 = Vector3.Transform(new Vector3(num3 + 1, num4 + 1, 0f), m);
				AppendImageExtrusionRectangle(p, p2, p3, p4, forward, flip, Color.MultiplyColorOnly(color, s));
			}
		}

		public void AppendImageExtrusionRectangle(Vector3 p11, Vector3 p21, Vector3 p12, Vector3 p22, Vector3 forward, bool flip, Color color)
		{
			int count = Vertices.Count;
			Vertices.Count += 4;
			int index = Vertices.Count - 4;
			BlockMeshVertex value = new()
			{
				Position = p11,
				TextureCoordinates = p11.XY + (forward.XY / 2f),
				Color = color
			};
            Vertices[index] = value;
			int index2 = Vertices.Count - 3;
			value = new BlockMeshVertex
			{
				Position = p21,
				TextureCoordinates = p21.XY + (forward.XY / 2f),
				Color = color
			};
            Vertices[index2] = value;
			int index3 = Vertices.Count - 2;
			value = new BlockMeshVertex
			{
				Position = p12,
				TextureCoordinates = p12.XY + (forward.XY / 2f),
				Color = color
			};
            Vertices[index3] = value;
			int index4 = Vertices.Count - 1;
			value = new BlockMeshVertex
			{
				Position = p22,
				TextureCoordinates = p22.XY + (forward.XY / 2f),
				Color = color
			};
            Vertices[index4] = value;
			Indices.Count += 6;
			if (flip)
			{
				Indices[^6] = count;
				Indices[^5] = count + 2;
				Indices[^4] = count + 1;
				Indices[^3] = count + 2;
				Indices[^2] = count + 3;
				Indices[^1] = count + 1;
			}
			else
			{
				Indices[^6] = count;
				Indices[^5] = count + 1;
				Indices[^4] = count + 2;
				Indices[^3] = count + 2;
				Indices[^2] = count + 1;
				Indices[^1] = count + 3;
			}
		}
	}
}
