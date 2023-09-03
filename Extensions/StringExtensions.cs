using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace YoLoTool.Extensions
{
    public static class StringExtensions
    {
        private static readonly MD5 _md5 = MD5.Create(); 

        public static Color GetLabelColor(this string name)
        {
            var hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(name));
            return Color.FromArgb(hash[2], hash[0], hash[1]);
        }
    }
}
