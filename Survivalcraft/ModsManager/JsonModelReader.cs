using System;
using System.Collections.Generic;
using Engine.Graphics;
using System.IO;
using Engine;
using SimpleJson;
namespace Game
{
    /** 
     * 此处基础坐标系为YZX
     * 
     **/
    public class JsonModelReader
    {
        public static Dictionary<string, List<Vector3>> FacesDic = new Dictionary<string, List<Vector3>>();
        public static Dictionary<string,Vector3> NormalDic = new Dictionary<string,Vector3>();
        static JsonModelReader()
        {
            FacesDic.Add("north", new List<Vector3>() { Vector3.UnitX, Vector3.Zero, Vector3.UnitY, new Vector3(1, 1, 0) });
            FacesDic.Add("south", new List<Vector3>() { new Vector3(1, 0, 1), Vector3.UnitZ, new Vector3(0, 1, 1), new Vector3(1, 1, 1) });

            FacesDic.Add("east", new List<Vector3>() { new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) });
            FacesDic.Add("west", new List<Vector3>() { Vector3.Zero, Vector3.UnitZ, new Vector3(0, 1, 1), Vector3.UnitY });

            FacesDic.Add("up", new List<Vector3>() { Vector3.UnitY, new Vector3(0, 1, 1), Vector3.One, new Vector3(1, 1, 0) });
            FacesDic.Add("down", new List<Vector3>() { Vector3.Zero, Vector3.UnitZ, new Vector3(1, 0, 1), new Vector3(1, 0, 0) });

            NormalDic.Add("north", new Vector3(0, 0, -1));
            NormalDic.Add("south", new Vector3(0, 0, 1));
            NormalDic.Add("east", new Vector3(1, 0, 0));
            NormalDic.Add("west", new Vector3(-1, 0, 0));
            NormalDic.Add("up", new Vector3(0, 1, 0));
            NormalDic.Add("down", new Vector3(0, -1, 0));
        }
        public static float ObjConvertFloat(object obj)
        {
            if (obj is double) return (float)(double)obj;
            //else if (obj is int) return (float)(int)obj;
            else if (obj is long) return (long)obj;
            throw new Exception("错误的数据转换，不能将" + obj.GetType().Name + "转换为float");
        }
        public static JsonModel Load(Stream stream)
        {
            Dictionary<string, ObjModelReader.ObjMesh> Meshes = new Dictionary<string, ObjModelReader.ObjMesh>();
            using (stream)
            {
                object obj = SimpleJson.SimpleJson.DeserializeObject(new StreamReader(stream).ReadToEnd());
                if (obj is JsonObject)
                {
                    JsonObject jsonObj = obj as JsonObject;
                    Vector2 textureSize = Vector2.Zero;
                    Dictionary<string, string> texturemap = new Dictionary<string, string>();
                    if (jsonObj.TryGetValue("textures", out object obj7))
                    {
                        JsonObject jobj5 = obj7 as JsonObject;
                        foreach (var item in jobj5)
                        {
                            texturemap.Add(item.Key, (string)item.Value);
                        }
                    }
                    if (jsonObj.TryGetValue("texture_size", out object obj2))
                    {
                        JsonArray array = obj2 as JsonArray;
                        textureSize = new Vector2(ObjConvertFloat(array[0]), ObjConvertFloat(array[1]));
                    }
                    if (jsonObj.TryGetValue("elements", out object jarr))
                    {
                        JsonArray array = jarr as JsonArray;
                        foreach (JsonObject jobj in array)
                        {
                            JsonArray from = jobj["from"] as JsonArray;
                            JsonArray to = jobj["to"] as JsonArray;
                            string name = "undefined";
                            if (jobj.TryGetValue("name", out object obj8)) {
                                name = obj8 as string;
                            }
                            if (Meshes.TryGetValue(name, out ObjModelReader.ObjMesh objMesh) == false)
                            {
                                objMesh = new ObjModelReader.ObjMesh();
                                objMesh.MeshName = name;
                                Meshes.Add(name, objMesh);
                            }
                            Vector3 start = new Vector3(ObjConvertFloat(from[0]), ObjConvertFloat(from[1]), ObjConvertFloat(from[2]));
                            Vector3 end = new Vector3(ObjConvertFloat(to[0]), ObjConvertFloat(to[1]), ObjConvertFloat(to[2]));
                            Matrix transform = Matrix.CreateScale(end.X - start.X, end.Y - start.Y, end.Z - start.Z) * Matrix.CreateTranslation(start.X, start.Y, start.Z) * Matrix.CreateScale(0.0625f);//基础缩放变换
                            if (jobj.TryGetValue("faces", out object obj3))
                            {//每个面，开始生成六个面的顶点数据
                                JsonObject jsonobj2 = obj3 as JsonObject;
                                foreach (var jobj2 in jsonobj2)
                                {
                                    ObjModelReader.ObjMesh childMesh = new ObjModelReader.ObjMesh();
                                    List<Vector3> vectors = FacesDic[jobj2.Key];//预取出四个面的点
                                    JsonObject jobj3 = jobj2.Value as JsonObject;
                                    float rotate = 0f;
                                    float[] uvs = new float[4];
                                    if (jobj3.TryGetValue("uv", out object obj4))
                                    {//处理uv坐标数据
                                        JsonArray uvarr = obj4 as JsonArray;
                                        for (int k = 0; k < uvarr.Count; k++)
                                        {
                                            uvs[k] = ObjConvertFloat(uvarr[k]);
                                        }
                                    }
                                    if (jobj3.TryGetValue("rotation", out object obj6))
                                    {//处理uv旋转数据
                                        //rotate = MathUtils.DegToRad(ObjConvertFloat(obj6));
                                    }
                                    if (jobj3.TryGetValue("texture", out object obj5))
                                    {//处理贴图数据
                                        string tkey = obj5 as string;
                                        if (texturemap.TryGetValue(tkey.Substring(1),out string path)) {
                                            childMesh.TexturePath = path;
                                        }
                                    }
                                    childMesh.MeshName = jobj2.Key;
                                    ObjModelReader.ObjPosition[] ops = new ObjModelReader.ObjPosition[3];
                                    ObjModelReader.ObjTexCood[] ots = new ObjModelReader.ObjTexCood[3];
                                    ObjModelReader.ObjNormal[] ons = new ObjModelReader.ObjNormal[3];
                                    //生成第一个三角面顶点
                                    Vector3 p1 = Vector3.Transform(vectors[0], transform);
                                    Vector3 p2 = Vector3.Transform(vectors[1], transform);
                                    Vector3 p3 = Vector3.Transform(vectors[2], transform);
                                    ops[0] = new ObjModelReader.ObjPosition(p1.X, p1.Y, p1.Z);
                                    ops[1] = new ObjModelReader.ObjPosition(p2.X, p2.Y, p2.Z);
                                    ops[2] = new ObjModelReader.ObjPosition(p3.X, p3.Y, p3.Z);
                                    //生成第一个三角面的纹理坐标
                                    Vector2 t1 = new Vector2(uvs[0] / 16f, uvs[3] / 16f);
                                    Vector2 t2 = new Vector2(uvs[2] / 16f, uvs[3] / 16f);
                                    Vector2 t3 = new Vector2(uvs[2] / 16f, uvs[1] / 16f);
                                    //t1 = Vector2.Transform(t1, Matrix.CreateRotationX(rotate));
                                    //t2 = Vector2.Transform(t2, Matrix.CreateRotationX(rotate));
                                    //t3 = Vector2.Transform(t3, Matrix.CreateRotationX(rotate));
                                    ots[0] = new ObjModelReader.ObjTexCood(t1.X, t1.Y);
                                    ots[1] = new ObjModelReader.ObjTexCood(t2.X, t2.Y);
                                    ots[2] = new ObjModelReader.ObjTexCood(t3.X, t3.Y);
                                    //生成第一个三角面的顶点法线
                                    Vector3 normal = NormalDic[jobj2.Key];
                                    ons[0] = new ObjModelReader.ObjNormal(normal.X, normal.Y, normal.Z);
                                    ons[1] = new ObjModelReader.ObjNormal(normal.X, normal.Y, normal.Z);
                                    ons[2] = new ObjModelReader.ObjNormal(normal.X, normal.Y, normal.Z);
                                    int startcount = childMesh.Vertices.Count;
                                    childMesh.Indices.Add((ushort)(startcount++));
                                    childMesh.Indices.Add((ushort)(startcount++));
                                    childMesh.Indices.Add((ushort)(startcount++));
                                    childMesh.Vertices.Add(new ObjModelReader.ObjVertex() { position = ops[0], objNormal = ons[0], texCood = ots[0] });
                                    childMesh.Vertices.Add(new ObjModelReader.ObjVertex() { position = ops[1], objNormal = ons[1], texCood = ots[1] });
                                    childMesh.Vertices.Add(new ObjModelReader.ObjVertex() { position = ops[2], objNormal = ons[2], texCood = ots[2] });
                                    //生成第二个三角面
                                    p1 = Vector3.Transform(vectors[0], transform);
                                    p2 = Vector3.Transform(vectors[2], transform);
                                    p3 = Vector3.Transform(vectors[3], transform);
                                    ops[0] = new ObjModelReader.ObjPosition(p1.X, p1.Y, p1.Z);
                                    ops[1] = new ObjModelReader.ObjPosition(p2.X, p2.Y, p2.Z);
                                    ops[2] = new ObjModelReader.ObjPosition(p3.X, p3.Y, p3.Z);
                                    //生成第二个三角面的纹理坐标
                                    t1 = new Vector2(uvs[0] / 16f, uvs[3] / 16f);
                                    t2 = new Vector2(uvs[2] / 16f, uvs[1] / 16f);
                                    t3 = new Vector2(uvs[0] / 16f, uvs[1] / 16f);
                                    //t1 = Vector2.Transform(t1, Matrix.CreateRotationX(rotate));
                                    //t2 = Vector2.Transform(t2, Matrix.CreateRotationX(rotate));
                                    //t3 = Vector2.Transform(t3, Matrix.CreateRotationX(rotate));
                                    ots[0] = new ObjModelReader.ObjTexCood(t1.X, t1.Y);
                                    ots[1] = new ObjModelReader.ObjTexCood(t2.X, t2.Y);
                                    ots[2] = new ObjModelReader.ObjTexCood(t3.X, t3.Y);
                                    //生成第二个三角面的顶点法线
                                    ons[0] = new ObjModelReader.ObjNormal(normal.X, normal.Y, normal.Z);
                                    ons[1] = new ObjModelReader.ObjNormal(normal.X, normal.Y, normal.Z);
                                    ons[2] = new ObjModelReader.ObjNormal(normal.X, normal.Y, normal.Z);
                                    childMesh.Indices.Add((ushort)(startcount++));
                                    childMesh.Indices.Add((ushort)(startcount++));
                                    childMesh.Indices.Add((ushort)(startcount++));
                                    childMesh.Vertices.Add(new ObjModelReader.ObjVertex() { position = ops[0], objNormal = ons[0], texCood = ots[0] });
                                    childMesh.Vertices.Add(new ObjModelReader.ObjVertex() { position = ops[1], objNormal = ons[1], texCood = ots[1] });
                                    childMesh.Vertices.Add(new ObjModelReader.ObjVertex() { position = ops[2], objNormal = ons[2], texCood = ots[2] });
                                    objMesh.ChildMeshes.Add(childMesh);
                                }
                            }
                        }
                    }
                }
            }
            return ObjModelReader.ObjMeshesToModel<JsonModel>(Meshes);
        }
    }

    public class JsonModel : Model { }
}
