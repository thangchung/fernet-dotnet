using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace Fernet
{
    /// <summary>
    /// SimpleFernet implements Fernet spec at nhttps://github.com/fernet/spec/blob/master/Spec.md,
    /// mainly base on the idea from https://stackoverflow.com/questions/50843953/decrypt-python-cryptography-fernet-token-in-c-sharp,
    /// but we have modified a lot :) 
    /// </summary>
    public class SimpleFernet
    {
        public static string Encrypt(byte[] key, byte[] data, DateTime? timestamp = null, byte[] iv = null,
            bool trimEnd = false)
        {
            if (key == null)
            {
                throw new FernetException($"{nameof(key)} is null.");
            }

            if (key.Length != 32)
            {
                throw new FernetException($"Length of {nameof(key)} should be 32.");
            }

            if (data == null)
            {
                throw new FernetException($"{nameof(data)} is null.");
            }

            if (iv != null && iv.Length != 16)
            {
                throw new FernetException($"Length of {nameof(iv)} should be 16.");
            }

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

            return result.UrlSafe64Encode(trimEnd);
        }

        // Token is base64 url encoded
        public static byte[] Decrypt(byte[] key, string token, out DateTime timestamp, int? ttl = null)
        {
            if (key == null)
            {
                throw new FernetException($"{nameof(key)} is null.");
            }

            if (key.Length != 32)
            {
                throw new FernetException($"Length of {nameof(key)} should be 32.");
            }

            if (token == null)
            {
                throw new FernetException($"{nameof(key)} is null.");
            }

            var token2 = token.UrlSafe64Decode();

            if (token2.Length < 57) throw new FernetException($"Length of {nameof(key)} should be greater or equal 57.");

            var version = token2[0];

            if (version != 0x80) throw new FernetException("Invalid version.");

            var signingKey = new byte[16];
            Buffer.BlockCopy(key, 0, signingKey, 0, 16);

            using (var hmac = new HMACSHA256(signingKey))
            {
                hmac.TransformFinalBlock(token2, 0, token2.Length - 32);
                var hash2 = hmac.Hash;

                var hash = token2.Skip(token2.Length - 32).Take(32);

                if (!hash.SequenceEqual(hash2)) throw new FernetException("Wrong HMAC!");
            }

            // BigEndian to LittleEndian
            var timestamp2 = BitConverter.ToInt64(token2, 1);
            timestamp2 = IPAddress.NetworkToHostOrder(timestamp2);
            var datetimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp2);
            timestamp = datetimeOffset.UtcDateTime;

            // calculate TTL
            if (ttl.HasValue)
            {
                var calculatedTimeSeconds = datetimeOffset.ToUnixTimeSeconds() + ttl.Value;
                var currentTimeSeconds = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                if (calculatedTimeSeconds < currentTimeSeconds)
                {
                    throw new FernetException("Token is expired.");
                }
            }

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

        public static string GenerateKey()
        {
            var keyBytes = new byte[32];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(keyBytes);
            return keyBytes.UrlSafe64Encode();
        }
    }
}
