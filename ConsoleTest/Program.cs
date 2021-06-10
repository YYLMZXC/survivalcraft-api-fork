using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
namespace ConsoleTest
{
    public class Ivector2 : IComparable<Ivector2>
    {
        public float X;
        public float Y;
        public Ivector2(float _,float __) {
            X = _;
            Y = __;
        }
        public int CompareTo(Ivector2 other)
        {
            if (other.X < X) return 1;
            else if (other.X == X)
            {
                if (other.Y < Y) return 1;
                else if (other.Y == Y) return 0;
                else return -1;
            }
            else {
                return -1;
            }
        }
    }

    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Ivector2 v1 = new Ivector2(0f, 1f);
            Ivector2 v2 = new Ivector2(0f, -1f);
            Ivector2 v3 = new Ivector2(0f, 0f);
            SortedSet<Ivector2> vectors = SortVector2(v1,v2,v3);
        }

        public static SortedSet<Ivector2> SortVector2(Ivector2 v1, Ivector2 v2, Ivector2 v3)
        {
            SortedSet<Ivector2> vectors = new SortedSet<Ivector2>();
            vectors.Add(v1);
            vectors.Add(v2);
            vectors.Add(v3);
            return vectors;
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
