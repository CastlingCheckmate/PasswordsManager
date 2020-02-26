using System;

namespace Extensions.ByteArrayExtensions
{

    public static class ByteArrayExtensions
    {

        public static string ToHexadecimalString(this byte[] bytes)
        {
            return string.Join(Environment.NewLine, BitConverter.ToString(bytes).Replace("-", string.Empty));
        }

    }

}