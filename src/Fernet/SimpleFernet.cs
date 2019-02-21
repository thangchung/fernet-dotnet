using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Fernet.Internal;

namespace Fernet
{
    /// <summary>
    ///     https://github.com/fernet/spec/blob/master/Spec.md
    /// </summary>
    public class SimpleFernet
    {
        public static string EncryptFernet(byte[] key, byte[] data, DateTime? timestamp = null, byte[] iv = null,
            bool trimEnd = false)
        {
            Guard.NotNull(key, $"{nameof(key)} is null.");
            Guard.Equal(key.Length, 32, $"Length of {nameof(key)} should be 32.");
            Guard.NotNull(data, $"{nameof(data)} is null.");
            Guard.NotEqual(iv != null && iv.Length != 16, true, $"Length of {nameof(iv)} should be 16.");

            if (timestamp == null) timestamp = DateTime.UtcNow;

            var result = new byte[57 + (data.Length + 16) / 16 * 16];

            result[0] = 0x80;

            // BigEndian to LittleEndian
            var timestamp2 = new DateTimeOffset(timestamp.Value).ToUnixTimeSeconds();
            timestamp2 = IPAddress.NetworkToHostOrder(timestamp2);
            var timestamp3 = BitConverter.GetBytes(timestamp2);
            Buffer.BlockCopy(timestamp3, 0, result, 1, timestamp3.Length);

            using (var aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;

                var encryptionKey = new byte[16];
                Buffer.BlockCopy(key, 16, encryptionKey, 0, 16);

                aes.Key = encryptionKey;

                if (iv != null)
                    aes.IV = iv;
                else
                    aes.GenerateIV();

                Buffer.BlockCopy(aes.IV, 0, result, 9, 16);

                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
                    Buffer.BlockCopy(encrypted, 0, result, 25, encrypted.Length);
                }
            }

            var signingKey = new byte[16];
            Buffer.BlockCopy(key, 0, signingKey, 0, 16);

            using (var hmac = new HMACSHA256(signingKey))
            {
                hmac.TransformFinalBlock(result, 0, result.Length - 32);
                Buffer.BlockCopy(hmac.Hash, 0, result, result.Length - 32, 32);
            }

            return result.Base64UrlEncode(trimEnd);
        }

        // Token is base64 url encoded
        public static byte[] DecryptFernet(byte[] key, string token, out DateTime timestamp)
        {
            Guard.NotNull(key, $"{nameof(key)} is null.");
            Guard.Equal(key.Length, 32, $"Length of {nameof(key)} should be 32.");
            Guard.NotNull(token, $"{nameof(key)} is null.");

            var token2 = token.Base64UrlDecode();

            if (token2.Length < 57) throw new ArgumentException(nameof(token));

            var version = token2[0];

            if (version != 0x80) throw new Exception("version");

            var signingKey = new byte[16];
            Buffer.BlockCopy(key, 0, signingKey, 0, 16);

            using (var hmac = new HMACSHA256(signingKey))
            {
                hmac.TransformFinalBlock(token2, 0, token2.Length - 32);
                var hash2 = hmac.Hash;

                var hash = token2.Skip(token2.Length - 32).Take(32);

                if (!hash.SequenceEqual(hash2)) throw new Exception("Wrong HMAC!");
            }

            // BigEndian to LittleEndian
            var timestamp2 = BitConverter.ToInt64(token2, 1);
            timestamp2 = IPAddress.NetworkToHostOrder(timestamp2);
            timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp2).UtcDateTime;

            byte[] decrypted;

            using (var aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var encryptionKey = new byte[16];
                Buffer.BlockCopy(key, 16, encryptionKey, 0, 16);
                aes.Key = encryptionKey;

                var iv = new byte[16];
                Buffer.BlockCopy(token2, 9, iv, 0, 16);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    const int startCipherText = 25;
                    var cipherTextLength = token2.Length - 32 - 25;
                    decrypted = decryptor.TransformFinalBlock(token2, startCipherText, cipherTextLength);
                }
            }

            return decrypted;
        }
    }

    public static class Base64StringExtensions
    {
        public static string Base64UrlEncode(this byte[] bytes, bool trimEnd = false)
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

        public static byte[] Base64UrlDecode(this string s)
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
    }

    public static class ByteExtensions
    {
        public static byte[] GetBytes(this string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(this byte[] bytes)
        {
            var size = (int)Math.Ceiling(((decimal)bytes.Length) / sizeof(char));
            var chars = new char[size];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
