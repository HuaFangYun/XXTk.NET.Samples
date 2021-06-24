using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.ShortUrl.Api.Converters
{
    public static class Base62Converter
    {
        private static readonly char[] _baseChars
            = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly Dictionary<char, int> _charValues
            = _baseChars
            .Select((c, i) => new { Char = c, Index = i })
            .ToDictionary(c => c.Char, c => c.Index);

        public static string LongToBase(long value)
        {
            var targetBase = _baseChars.Length;
            var buffer = new char[Math.Max((int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

            var i = buffer.Length;
            do
            {
                buffer[--i] = _baseChars[value % targetBase];
                value /= targetBase;
            }
            while (value > 0);

            return new string(buffer, i, buffer.Length - i);
        }

        public static long BaseToLong(string number)
        {
            var chrs = number.ToCharArray();
            var m = chrs.Length - 1;
            var n = _baseChars.Length;
            int x;
            var result = 0L;
            for (var i = 0; i < chrs.Length; i++)
            {
                x = _charValues[chrs[i]];
                result += x * (long)Math.Pow(n, m--);
            }

            return result;
        }
    }

}
