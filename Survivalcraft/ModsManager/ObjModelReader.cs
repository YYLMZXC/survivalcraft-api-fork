using Engine.Graphics;
using System.Collections.Generic;
using System.IO;
using Engine;
using Engine.Media;
namespace Game
{
    public class ObjModelReader
    {
        public struct ObjPosition
        {
            public float x, y, z;
            public ObjPosition(string x_, string y_, string z_)
            {
                x = float.Parse(x_);
                y = float.Parse(y_);
                z = float.Parse(z_);
            }

        }
        public struct ObjVertex {
            public ObjPosition position;
            public ObjTexCood texCood;
        }
        public struct ObjNormal
        {
            public float x, y, z;
            public ObjNormal(string x_, string y_, string z_) {
                x = float.Parse(x_);
                y = float.Parse(y_);
                z = float.Parse(z_);
            }
        }

        public struct ObjTexCood {
            public float tx, ty;
            public ObjTexCood(string tx_, string ty_) {
                tx = float.Parse(tx_);
                ty = float.Parse(ty_);
            }
        }
        public class ObjMesh {
            public DynamicArray<ObjVertex> Vertices = new DynamicArray<ObjVertex>();
            public DynamicArray<ushort> Indices = new DynamicArray<ushort>();
            public BoundingBox CalculateBoundingBox()
            {
                List<Vector3> vectors = new List<Vector3>();
                for (int i=0;i<Vertices.Count;i++) {
                    vectors.Add(new Vector3(Vertices[i].position.x, Vertices[i].position.y, Vertices[i].position.z));
                }
                return new BoundingBox(vectors);
            }
        }

        public static Model Load(Stream stream)
        {
            Dictionary<string, ObjMesh> Meshes = new Dictionary<string, ObjMesh>();
            List<ObjPosition> objPositions = new List<ObjPosition>();
            List<ObjTexCood> objTexCoods = new List<ObjTexCood>();
            List<ObjNormal> objNormals = new List<ObjNormal>();
            using (stream)
            {
                StreamReader streamReader = new StreamReader(stream);
                ObjMesh objMesh = null;
                while (streamReader.EndOfStream == false)
                {
                    string line = streamReader.ReadLine();
                    string[] spl = line.Split(new char[] { (char)0x09, (char)0x20 }, System.StringSplitOptions.None);
                    switch (spl[0])
                    {
                        case "o":
                            {
                                if (Meshes.TryGetValue(spl[1], out ObjMesh mesh))
                                {
                                    objMesh = mesh;
                                }
                                else
                                {
                                    objMesh = new ObjMesh();
                                    Meshes.Add(spl[1], objMesh);
                                }
                                break;

                            }
                        case "v":
                            {
                                objPositions.Add(new ObjPosition(spl[1], spl[2], spl[3]));
                                break;
                            }
                        case "vt":
                            {
                                objTexCoods.Add(new ObjTexCood(spl[1], spl[2]));
                                break;
                            }
                        case "vn":
                            {
                                objNormals.Add(new ObjNormal(spl[1], spl[2], spl[3]));
                                break;
                            }
                        case "f":
                            {
                                int SideCount = spl.Length - 1;
                                if (SideCount != 3) { throw new System.Exception("模型必须为三角面"); }
                                else
                                {
                                    int i = 0;
                                    while (++i < spl.Length)
                                    {
                                        string[] param = spl[i].Split(new char[] { '/' }, System.StringSplitOptions.None);
                                        if (param.Length != 3) { throw new System.Exception("面参数错误"); }
                                        else
                                        {
                                            int pa = int.Parse(param[0]);//顶点索引
                                            int pb = int.Parse(param[1]);//纹理索引
                                            int pc = int.Parse(param[2]);//法线索引
                                            ObjPosition objPosition = objPositions[pa - 1];
                                            ObjTexCood texCood = objTexCoods[pb - 1];
                                            objMesh.Indices.Add((ushort)objMesh.Vertices.Count);
                                            objMesh.Vertices.Add(new ObjVertex() { position = objPosition, texCood = texCood });
                                        }
                                    }
                                }
                                break;
                            }
                        default: break;
                    }
                }
            }
            Model Model = new Model();
            ModelBone rootBone = Model.NewBone("Object", Matrix.Identity, null);
            foreach (var c in Meshes)
            {
                ModelBone modelBone = Model.NewBone(c.Key, Matrix.Identity, rootBone);
                ModelMesh mesh = Model.NewMesh(c.Key, modelBone, c.Value.CalculateBoundingBox());
                VertexBuffer vertexBuffer = new VertexBuffer(new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementSemantic.Position), new VertexElement(12, VertexElementFormat.Vector2, VertexElementSemantic.TextureCoordinate)),c.Value.Vertices.Count);
                vertexBuffer.SetData(c.Value.Vertices.Array, 0, c.Value.Vertices.Count);
                IndexBuffer indexBuffer = new IndexBuffer(IndexFormat.SixteenBits, c.Value.Indices.Count);
                indexBuffer.SetData(c.Value.Indices.Array, 0, c.Value.Indices.Count);
                mesh.NewMeshPart(vertexBuffer, indexBuffer, 0, c.Value.Indices.Count, c.Value.CalculateBoundingBox());
                Model.AddMesh(mesh);
            }
            return Model;
        }




    }
}
