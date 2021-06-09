using Engine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Engine.Graphics;

namespace Game
{
    public class ModelExport
    {
        public class ObjText {
            public string obj;
            public string mtl;
        }
        public class Tex {
            public Color Color;
            public Vector2 T1;
            public Vector2 T2;
            public Vector2 T3;
            public int Index;
            public int hash;
            public Tex(Vector2 a1, Vector2 a2, Vector2 a3,Color c) {
                T1 = a1;T2 = a2;T3 = a3;Color = c;
                hash =  c.GetHashCode();
            }
        }

        public static Dictionary<Point2,List<TerrainGeometrySubset>> geometries = new Dictionary<Point2, List<TerrainGeometrySubset>>();
        public static List<Tex> textures = new List<Tex>();
        public static SubsystemBlocksTexture SubsystemBlocksTexture;
        public static string ExportObjModel(TerrainGeometrySubset terrainVertices, int ID)
        {
            List<Vector3> vs = new List<Vector3>();//顶点
            List<Vector2> vts = new List<Vector2>();//纹理
            List<string> v_string = new List<string>();//顶点
            List<string> vt_string = new List<string>();//纹理
            List<string> vn_string = new List<string>();//法线
            List<string> fs = new List<string>();//面

            DynamicArray<TerrainVertex> Vertices = terrainVertices.Vertices;
            DynamicArray<ushort> Indices = terrainVertices.Indices;
            if (Vertices.Count == 0 || Indices.Count == 0) return string.Empty;
            for (int i = 0; i < Vertices.Count; i++)
            {
                TerrainVertex vertex = Vertices[i];
                Vector3 vector3 = new Vector3(vertex.X, vertex.Y, vertex.Z);
                Vector2 vector2 = new Vector2(vertex.Tx / 32767f, 1f - vertex.Ty / 32767f);                
                vs.Add(vector3);//添加坐标
                vts.Add(vector2);//添加纹理坐标
                v_string.Add($"v {vertex.X} {vertex.Y} {vertex.Z}\n");
                vt_string.Add($"vt {vector2.X.ToString("F6")} {vector2.Y.ToString("F6")}\n");
            }
            vn_string.Add($"vn 0 -1 0\n");
            for (int i = 0; i < Indices.Count; i += 3)
            {
                int index = Indices[i] + 1;
                int index2 = Indices[i + 1] + 1;
                int index3 = Indices[i + 2] + 1;
                fs.Add($"f {index}/{index}/{vn_string.Count} {index2}/{index2}/{vn_string.Count} {index3}/{index3}/{vn_string.Count}\n");
                //添加贴图
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"o TerrainModelExport{ID}\n");
            foreach (string asas in v_string) { stringBuilder.Append(asas); }
            foreach (string asas in vt_string) { stringBuilder.Append(asas); }
            foreach (string asas in vn_string) { stringBuilder.Append(asas); }
            fs.Add($"usemtl none\n");
            foreach (string asas in fs) { stringBuilder.Append(asas); }
            return stringBuilder.ToString();
        }
        public static void Add(TerrainChunk terrainChunk) {            
            int yc = 0;
            List<TerrainVertex> terrainVertices = new List<TerrainVertex>();
            List<ushort> vs1 = new List<ushort>();
            if (geometries.TryGetValue(terrainChunk.Coords, out List<TerrainGeometrySubset> list))
            {
                return;
            }
            else {
                list = new List<TerrainGeometrySubset>();
                geometries.Add(terrainChunk.Coords, list);
            }
            foreach (TerrainChunkSliceGeometry buffer in terrainChunk.Geometry.Slices) {
                terrainVertices.AddRange(buffer.SubsetOpaque.Vertices);
                for (int i = 0; i < buffer.SubsetOpaque.Indices.Count; i++)
                {
                    vs1.Add((ushort)(yc + buffer.SubsetOpaque.Indices[i]));
                }
                yc = terrainVertices.Count;
                terrainVertices.AddRange(buffer.SubsetAlphaTest.Vertices);
                for (int i = 0; i < buffer.SubsetAlphaTest.Indices.Count; i++)
                {
                    vs1.Add((ushort)(yc + buffer.SubsetAlphaTest.Indices[i]));
                }
                yc = terrainVertices.Count;
                terrainVertices.AddRange(buffer.SubsetTransparent.Vertices);
                for (int i = 0; i < buffer.SubsetTransparent.Indices.Count; i++)
                {
                    vs1.Add((ushort)(yc + buffer.SubsetTransparent.Indices[i]));
                }
                for (int j = 0; j < 6; j++)
                {
                    yc = terrainVertices.Count;
                    terrainVertices.AddRange(buffer.OpaqueSubsetsByFace[j].Vertices);
                    for (int i = 0; i < buffer.OpaqueSubsetsByFace[j].Indices.Count; i++)
                    {
                        vs1.Add((ushort)(yc + buffer.OpaqueSubsetsByFace[j].Indices[i]));
                    }
                }
                for (int j = 0; j < 6; j++)
                {
                    yc = terrainVertices.Count;
                    terrainVertices.AddRange(buffer.AlphaTestSubsetsByFace[j].Vertices);
                    for (int i = 0; i < buffer.AlphaTestSubsetsByFace[j].Indices.Count; i++)
                    {
                        vs1.Add((ushort)(yc + buffer.AlphaTestSubsetsByFace[j].Indices[i]));
                    }
                }
                for (int j = 0; j < 6; j++)
                {
                    yc = terrainVertices.Count;
                    terrainVertices.AddRange(buffer.TransparentSubsetsByFace[j].Vertices);
                    for (int i = 0; i < buffer.TransparentSubsetsByFace[j].Indices.Count; i++)
                    {
                        vs1.Add((ushort)(yc + buffer.TransparentSubsetsByFace[j].Indices[i]));
                    }
                }
            }
            TerrainGeometrySubset terrainGeometry = new TerrainGeometrySubset();
            terrainGeometry.Vertices.AddRange(terrainVertices);
            terrainGeometry.Indices.AddRange(vs1);
            if (terrainGeometry.Vertices.Count > 0) list.Add(terrainGeometry);
        }
        public static Vector2 MinVec2(Vector2 v1,Vector2 v2,Vector2 v3) {
            Vector2 v4 = Vector2.Min(v1, v2);
            return Vector2.Min(v4, v3);
        }
        public static void Clear() {            
            geometries.Clear();
        }
        public static void Export() {
            StringBuilder s = new StringBuilder();            
            int ac = 0;
            string filename = $"chunk.obj";
            foreach (var list in geometries.Values)
            {
                foreach (var geom in list)
                {
                    s.Append(ExportObjModel(geom, ac));
                    ac++;
                }
            }
            //导出材质
            int bc = 1;
            foreach (Tex tex in textures) {
                RenderTexture($"Export/t{bc++}", tex.T1, tex.T2, tex.T3, tex.Color);
            }

            if (!Storage.FileExists("app:/Export/" + filename))
            {
                if (!Storage.DirectoryExists("app:/Export")) Storage.CreateDirectory("app:/Export");
                File.WriteAllText(Storage.GetSystemPath("app:/Export/" + filename), s.ToString());
            }
            Clear();
        }
        public static void RenderTexture(string txname, Vector2 tx1, Vector2 tx2, Vector2 tx3,Color color) {
            if (GameManager.Project != null) {
                if(SubsystemBlocksTexture==null)SubsystemBlocksTexture = GameManager.Project.FindSubsystem<SubsystemBlocksTexture>();
                PrimitivesRenderer2D primitivesRenderer2D = new PrimitivesRenderer2D();
                Texture2D texture2D = SubsystemBlocksTexture.BlocksTexture;
                RenderTarget2D render = new RenderTarget2D(texture2D.Width,texture2D.Height,1,ColorFormat.Rgba8888,DepthFormat.None);
                RenderTarget2D renderTarget2D = Display.RenderTarget;
                Display.RenderTarget = render;
                TexturedBatch2D texturedBatch2D = primitivesRenderer2D.TexturedBatch(texture2D,true);
                texturedBatch2D.QueueQuad(Vector2.Zero,new Vector2(texture2D.Width, texture2D.Height),1f,Vector2.Zero,Vector2.One,Color.White);
                Vector2 min = MinVec2(tx1,tx2,tx3);
                float x = min.X * 32;
                float y = min.Y * 32;
                texturedBatch2D.QueueTriangle(new Vector2(tx1.X * 32 - x, tx1.Y * 32 - y), new Vector2(tx2.X * 32 - x, tx2.Y * 32 - y), new Vector2(tx3.X * 32 - x, tx3.Y * 32 - y), 1f, tx1, tx2, tx3, color == Color.Transparent ? Color.White : color);
                primitivesRenderer2D.Flush();
                Display.RenderTarget = renderTarget2D;
                ModsManager.SaveToImage(txname, render);
            }        
        }
    }
}
