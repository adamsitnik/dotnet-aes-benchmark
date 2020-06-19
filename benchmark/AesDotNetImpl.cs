namespace benchmark
{
    using System.IO;
    using System.Security.Cryptography;

    public static class AesDotNetImpl
    {
        public static long DecryptAes(byte[] encrypted, byte[] key, byte[] iv, out byte[] decrypted)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;

                using (var decryptor = aes.CreateDecryptor(key, iv))
                {
                    var msEncrypted = new MemoryStream(encrypted);
                    var msDecrypted = new MemoryStream((int)(encrypted.Length * 1.5));
                    using (var decryptorStream = new CryptoStream(msEncrypted, decryptor, CryptoStreamMode.Read))
                    {
                        decryptorStream.CopyTo(msDecrypted);
                    }

                    decrypted = msDecrypted.ToArray();
                }
            }

            return 0;
        }

        public static byte[] Encrypt(byte[] bytes, byte[] key, byte[] iv)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;

                using (var encryptor = aes.CreateEncryptor(key, iv))
                {
                    var msEncrypted = new MemoryStream((int)(bytes.Length * 1.5));
                    using (var encryptorStream = new CryptoStream(msEncrypted, encryptor, CryptoStreamMode.Write))
                    {
                        encryptorStream.Write(bytes, 0, bytes.Length);
                        encryptorStream.Flush();
                    }

                    encrypted = msEncrypted.ToArray();
                }
            }

            return encrypted;
        }
    }
}
