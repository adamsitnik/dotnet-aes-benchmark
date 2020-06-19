namespace benchmark
{
    using System.Security.Cryptography;

    public static class AesDotNetImpl2
    {
        public static long DecryptAes(byte[] encrypted, byte[] key, byte[] iv, out byte[] decrypted)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                using (var decryptor = aes.CreateDecryptor(key, iv))
                {
                    decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                }
            }

            return 0;
        }
    }
}
