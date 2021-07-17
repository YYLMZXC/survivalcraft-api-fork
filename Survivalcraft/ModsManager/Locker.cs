using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
namespace Game
{

    public class Locker
    {
        public static string Words="0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Encrypt(string str) {
            var stringBuilder = new StringBuilder();
            int Len = Words.Length;
            byte[] data = Encoding.UTF8.GetBytes(str);
            for (int i=0;i< data.Length;i++) {
                int q = data[i] / Len;
                int r = data[i] % Len;
                if (q > 0) stringBuilder.Append(Words[q]);
                stringBuilder.Append(Words[r]);                       
            }
            return stringBuilder.ToString();
        }
    }
}
