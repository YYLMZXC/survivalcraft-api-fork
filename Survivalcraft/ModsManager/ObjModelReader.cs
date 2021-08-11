using Engine.Graphics;
using System.Collections.Generic;
using System.IO;
using Engine;
using Engine.Media;
using System;

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
            public ObjPosition(float x_, float y_, float z_)
            {
                x = (x_);
                y = (y_);
                z = (z_);
            }

        }
        public struct ObjVertex {
            public ObjPosition position;
            public ObjNormal objNormal;
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
            public ObjNormal(float x_, float y_, float z_)
            {
                x = (x_);
                y = (y_);
                z = (z_);
            }
        }
        public struct ObjTexCood {
            public float tx, ty;
            public ObjTexCood(string tx_, string ty_) {
                tx = float.Parse(tx_);
                ty = float.Parse(ty_);
            }
            public ObjTexCood(float tx_, float ty_)
            {
                tx = (tx_);
                ty = (ty_);
            }
        }
        public class ObjMesh {
            public DynamicArray<ObjVertex> Vertices = new DynamicArray<ObjVertex>();
            public DynamicArray<ushort> Indices = new DynamicArray<ushort>();
            public string TexturePath = "Textures/ModelNone";//默认位置
            public string MeshName;
            public BoundingBox CalculateBoundingBox()
            {
                List<Vector3> vectors = new List<Vector3>();
                for (int i=0;i<Vertices.Count;i++) {
                    vectors.Add(new Vector3(Vertices[i].position.x, Vertices[i].position.y, Vertices[i].position.z));
                }
                return new BoundingBox(vectors);
            }
            public List<ObjMesh> ChildMeshes = new List<ObjMesh>();
        }
        public static ObjModel Load(Stream stream)
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
                                            ObjNormal objNormal = objNormals[pc - 1];
                                            objMesh.Indices.Add((ushort)objMesh.Vertices.Count);
                                            objMesh.Vertices.Add(new ObjVertex() { position = objPosition,objNormal= objNormal, texCood = texCood });
                                        }
                                    }
                                }
                                break;
                            }
                        default: break;
                    }
                }
            }
            return ObjMeshesToModel<ObjModel>(Meshes);
        }

        public static void AppendMesh(Model model,ModelBone rootBone, string texturepath, ObjMesh objMesh)
        {
            ModelBone modelBone = model.NewBone(objMesh.MeshName, Matrix.Identity, rootBone);
            if (objMesh.Vertices.Count > 0)
            {
                ModelMesh mesh = model.NewMesh(objMesh.MeshName, modelBone, objMesh.CalculateBoundingBox());
                VertexBuffer vertexBuffer = new VertexBuffer(new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementSemantic.Position),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementSemantic.Normal),
                   new VertexElement(24, VertexElementFormat.Vector2, VertexElementSemantic.TextureCoordinate)), objMesh.Vertices.Count);
                MemoryStream stream1 = new MemoryStream();
                MemoryStream stream2 = new MemoryStream();
                BinaryWriter binaryWriter1 = new BinaryWriter(stream1);
                BinaryWriter binaryWriter2 = new BinaryWriter(stream2);
                for (int i = 0; i < objMesh.Vertices.Count; i++)
                {
                    ObjVertex objVertex = objMesh.Vertices[i];
                    binaryWriter1.Write(objVertex.position.x);
                    binaryWriter1.Write(objVertex.position.y);
                    binaryWriter1.Write(objVertex.position.z);
                    binaryWriter1.Write(objVertex.objNormal.x);
                    binaryWriter1.Write(objVertex.objNormal.y);
                    binaryWriter1.Write(objVertex.objNormal.z);
                    binaryWriter1.Write(objVertex.texCood.tx);
                    binaryWriter1.Write(objVertex.texCood.ty);
                }
                for (int i = 0; i < objMesh.Indices.Count; i++)
                {
                    binaryWriter2.Write(objMesh.Indices[i]);
                }
                byte[] vs = stream1.ToArray();
                byte[] ins = stream2.ToArray();
                stream1.Close();
                stream2.Close();
                vertexBuffer.SetData(objMesh.Vertices.Array, 0, objMesh.Vertices.Count);
                vertexBuffer.Tag = vs;
                IndexBuffer indexBuffer = new IndexBuffer(IndexFormat.SixteenBits, objMesh.Indices.Count);
                indexBuffer.SetData(objMesh.Indices.Array, 0, objMesh.Indices.Count);
                indexBuffer.Tag = ins;
                ModelMeshPart modelMeshPart = mesh.NewMeshPart(vertexBuffer, indexBuffer, 0, objMesh.Indices.Count, objMesh.CalculateBoundingBox());
                modelMeshPart.TexturePath = objMesh.TexturePath;
                model.AddMesh(mesh);
            }
            foreach (ObjMesh objMesh1 in objMesh.ChildMeshes)
            {
                AppendMesh(model, modelBone, objMesh1.TexturePath, objMesh1);
            }
        }

        public static T ObjMeshesToModel<T>(Dictionary<string,ObjMesh> Meshes) where T:class {
            Type ctype = typeof(T);
            if (!ctype.IsSubclassOf(typeof(Model))) throw new Exception("不能将" + ctype.Name + "转换为Model类型");
            object iobj = Activator.CreateInstance(ctype);
            Model Model = iobj as Model;
            ModelBone rootBone = Model.NewBone("Object", Matrix.Identity, null);
            foreach (var c in Meshes)
            {
                AppendMesh(Model, rootBone, c.Key, c.Value);
            }
            return iobj as T;
        }
    }

    public class ObjModel : Model { }
}
