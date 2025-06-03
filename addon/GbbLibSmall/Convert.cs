using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbbLibSmall2
{
    public class Convert
    {
        public static string ButesToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString().Trim();
        }

        public static byte[] StringToBytes(string str)
        {
            int length = str.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = System.Convert.ToByte(str.Substring(i, 2), 16);
            }
            return bytes;
        }

    }
}
