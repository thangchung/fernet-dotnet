using System;

namespace Fernet
{
    public static class Base64StringExtensions
    {
        public static string UrlSafe64Encode(this byte[] bytes, bool trimEnd = false)
        {
            try
            {
                var length = (bytes.Length + 2) / 3 * 4;
                var chars = new char[length];
                Convert.ToBase64CharArray(bytes, 0, bytes.Length, chars, 0);

                var trimmedLength = length;

                if (trimEnd)
                    switch (bytes.Length % 3)
                    {
                        case 1:
                            trimmedLength -= 2;
                            break;
                        case 2:
                            trimmedLength -= 1;
                            break;
                    }

                for (var i = 0; i < trimmedLength; i++)
                    switch (chars[i])
                    {
                        case '/':
                            chars[i] = '_';
                            break;
                        case '+':
                            chars[i] = '-';
                            break;
                    }

                var result = new string(chars, 0, trimmedLength);
                return result;
            }
            catch (Exception e)
            {
                throw new FernetException(e.Message, e);
            }
        }

        public static byte[] UrlSafe64Decode(this string s)
        {
            try
            {
                char[] chars;

                switch (s.Length % 4)
                {
                    case 2:
                        chars = new char[s.Length + 2];
                        chars[chars.Length - 2] = '=';
                        chars[chars.Length - 1] = '=';
                        break;
                    case 3:
                        chars = new char[s.Length + 1];
                        chars[chars.Length - 1] = '=';
                        break;
                    default:
                        chars = new char[s.Length];
                        break;
                }

                for (var i = 0; i < s.Length; i++)
                    switch (s[i])
                    {
                        case '_':
                            chars[i] = '/';
                            break;
                        case '-':
                            chars[i] = '+';
                            break;
                        default:
                            chars[i] = s[i];
                            break;
                    }

                var result = Convert.FromBase64CharArray(chars, 0, chars.Length);
                return result;
            }
            catch (Exception e)
            {
                throw new FernetException(e.Message, e);
            }
        }

        public static string ToBase64String(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64String(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
