using System.Collections.Generic;

namespace Engine.Graphics
{
	public  class FlatBatch3D : BaseFlatBatch
	{
        public FlatBatch3D()
        {
            base.DepthStencilState = DepthStencilState.Default;
            base.RasterizerState = RasterizerState.CullNoneScissor;
            base.BlendState = BlendState.AlphaBlend;
        }

        public void QueueBatchTriangles(FlatBatch3D batch, Matrix? matrix = null, Color? color = null)
        {
            int count = TriangleVertices.Count;
            TriangleVertices.AddRange(batch.TriangleVertices);
            int count2 = TriangleIndices.Count;
            int count3 = batch.TriangleIndices.Count;
            TriangleIndices.Count += count3;
            for (int i = 0; i < count3; i++)
            {
                TriangleIndices[i + count2] = (ushort)(batch.TriangleIndices[i] + count);
            }
            if (matrix.HasValue && matrix != Matrix.Identity)
            {
                TransformTriangles(matrix.Value, count);
            }
            if (color.HasValue && color != Color.White)
            {
                TransformTrianglesColors(color.Value, count);
            }
        }

        public void QueueBatchLines(FlatBatch3D batch, Matrix? matrix = null, Color? color = null)
        {
            int count = LineVertices.Count;
            LineVertices.AddRange(batch.LineVertices);
            int count2 = LineIndices.Count;
            int count3 = batch.LineIndices.Count;
            LineIndices.Count += count3;
            for (int i = 0; i < count3; i++)
            {
                LineIndices[i + count2] = (ushort)(batch.LineIndices[i] + count);
            }
            if (matrix.HasValue && matrix != Matrix.Identity)
            {
                TransformLines(matrix.Value, count);
            }
            if (color.HasValue && color != Color.White)
            {
                TransformLinesColors(color.Value, count);
            }
        }

        public void QueueBatch(FlatBatch3D batch, Matrix? matrix = null, Color? color = null)
        {
            QueueBatchLines(batch, matrix, color);
            QueueBatchTriangles(batch, matrix, color);
        }
		public void QueueLine(Vector3 p1, Vector3 p2, Color color)
		{
			int count = LineVertices.Count;
			LineVertices.Add(new VertexPositionColor(p1, color));
			LineVertices.Add(new VertexPositionColor(p2, color));
			LineIndices.Add(count);
			LineIndices.Add(count + 1);
		}

		public void QueueLine(Vector3 p1, Vector3 p2, Color color1, Color color2)
		{
			int count = LineVertices.Count;
			LineVertices.Add(new VertexPositionColor(p1, color1));
			LineVertices.Add(new VertexPositionColor(p2, color2));
			LineIndices.Add(count);
			LineIndices.Add(count + 1);
		}

		public void QueueLineStrip(IEnumerable<Vector3> points, Color color)
		{
			int count = LineVertices.Count;
			int num = 0;
			foreach (Vector3 point in points)
			{
				LineVertices.Add(new VertexPositionColor(point, color));
				num++;
			}
			for (int i = 0; i < num - 1; i++)
			{
				LineIndices.Add(count + i);
				LineIndices.Add(count + i + 1);
			}
		}

		public void QueueTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
		{
			int count = TriangleVertices.Count;
			TriangleVertices.Add(new VertexPositionColor(p1, color));
			TriangleVertices.Add(new VertexPositionColor(p2, color));
			TriangleVertices.Add(new VertexPositionColor(p3, color));
			TriangleIndices.Add(count);
			TriangleIndices.Add(count + 1);
			TriangleIndices.Add(count + 2);
		}

		public void QueueTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color1, Color color2, Color color3)
		{
			int count = TriangleVertices.Count;
			TriangleVertices.Add(new VertexPositionColor(p1, color1));
			TriangleVertices.Add(new VertexPositionColor(p2, color2));
			TriangleVertices.Add(new VertexPositionColor(p3, color3));
			TriangleIndices.Add(count);
			TriangleIndices.Add(count + 1);
			TriangleIndices.Add(count + 2);
		}

		public void QueueQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color)
		{
			int count = TriangleVertices.Count;
			TriangleVertices.Add(new VertexPositionColor(p1, color));
			TriangleVertices.Add(new VertexPositionColor(p2, color));
			TriangleVertices.Add(new VertexPositionColor(p3, color));
			TriangleVertices.Add(new VertexPositionColor(p4, color));
			TriangleIndices.Add(count);
			TriangleIndices.Add(count + 1);
			TriangleIndices.Add(count + 2);
			TriangleIndices.Add(count + 2);
			TriangleIndices.Add(count + 3);
			TriangleIndices.Add(count);
		}

		public void QueueBoundingBox(BoundingBox boundingBox, Color color)
		{
			QueueLine(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z), new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z), color);
			QueueLine(new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z), new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z), color);
			QueueLine(new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z), new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z), color);
			QueueLine(new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z), new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z), color);
			QueueLine(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z), new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z), new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z), new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z), new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z), new Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z), new Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z), new Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z), color);
			QueueLine(new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z), new Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z), color);
		}

		public void QueueBoundingFrustum(BoundingFrustum boundingFrustum, Color color)
		{
            ReadOnlyList<Vector3> array = boundingFrustum.Corners;
			QueueLine(array[0], array[1], color);
			QueueLine(array[1], array[2], color);
			QueueLine(array[2], array[3], color);
			QueueLine(array[3], array[0], color);
			QueueLine(array[4], array[5], color);
			QueueLine(array[5], array[6], color);
			QueueLine(array[6], array[7], color);
			QueueLine(array[7], array[4], color);
			QueueLine(array[0], array[4], color);
			QueueLine(array[1], array[5], color);
			QueueLine(array[2], array[6], color);
			QueueLine(array[3], array[7], color);
		}
	}
}
