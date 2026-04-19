using System.Security.Cryptography;
using System.Text;

namespace NashPay.API.Helpers
{
    public class EncryptionHelper
    {
        private readonly byte[] _encryptionKey;

        public EncryptionHelper(IConfiguration configuration)
        {
            var key = configuration["Encryption:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("Critical Error: Encryption Key is missing in configuration!");
            }

            // AES-256 requires 32 bytes
            _encryptionKey = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using Aes aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV(); // Unique IV for every encryption

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            
            // Write IV first, then encrypted data
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] buffer = Convert.FromBase64String(cipherText);
            using Aes aes = Aes.Create();
            aes.Key = _encryptionKey;

            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            Array.Copy(buffer, 0, iv, 0, ivLength);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer, ivLength, buffer.Length - ivLength);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
        }

        // SECURE RANDOM GENERATOR for API Keys
        public static string GenerateRandomKey(int length = 48)
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(length / 2));
        }
    }
}