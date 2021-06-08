using System;
using Engine;
using Engine.Graphics;
namespace ConsoleTest
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Vector3 v1 = new Vector3(1f, 1f, 0f);
            Vector3 v2 = new Vector3(0f, 1f, 0f);
            Vector3 v3 = new Vector3(0f, 1f, -1f);
            Vector3 vn = Cal_Normal_3D(v3,v2,v1);
        }


        /// <summary>
        /// 计算三点成面的法向量
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static Vector3 Cal_Normal_3D(Vector3 v1, Vector3 v2, Vector3 v3)
        {            
            float na = (v2.Y - v1.Y) * (v3.Z - v1.Z) - (v2.Z - v1.Z) * (v3.Y - v1.Y);
            float nb = (v2.Z - v1.Z) * (v3.X - v1.X) - (v2.X - v1.Z) * (v3.Z - v1.Z);
            float nc = (v2.X - v1.X) * (v3.Y - v1.Y) - (v2.Y - v1.Y) * (v3.X - v1.X);
            return Vector3.Zero - new Vector3(na, nb, nc);
        }

    }
}
