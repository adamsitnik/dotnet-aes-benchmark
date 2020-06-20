namespace benchmark_simple
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    class Program
    {
        static private readonly byte[] key128 = new byte[128 / 8];
        static private readonly byte[] iv = new byte[16];
        static private readonly byte[] ivCopy = new byte[16];

        private const int DataLength = 10000;
        static private readonly byte[] data = new byte[DataLength];

        static private byte[] encrypted128;

        static void Main(string[] args)
        {
            Random rnd = new Random(42);
            rnd.NextBytes(data);

            rnd.NextBytes(key128);

            rnd.NextBytes(iv);

            encrypted128 = Encrypt(data, key128, iv);

            byte[] decrypted;

            // warmup
            Console.WriteLine("Warm up");
            for (var i = 0; i < 10000; ++i)
            {
                decrypted = DecryptAesDotNetStream(encrypted128, key128, iv);
                if (!System.Linq.Enumerable.SequenceEqual(data, decrypted))
                {
                    Console.WriteLine("Decryption failed.");
                    Environment.Exit(1);
                }

                decrypted = DecryptAesDotNetTransform(encrypted128, key128, iv);
                if (!System.Linq.Enumerable.SequenceEqual(data, decrypted))
                {
                    Console.WriteLine("Decryption failed.");
                    Environment.Exit(1);
                }

                decrypted = DecryptAesBCrypt(encrypted128, key128, iv);
                if (!System.Linq.Enumerable.SequenceEqual(data, decrypted))
                {
                    Console.WriteLine("Decryption failed.");
                    Environment.Exit(1);
                }
            }

            const int repeats = 1000000;
            long startTicks;

            Console.WriteLine("Measure");

            // measure DecryptAesBCrypt
            startTicks = DateTime.UtcNow.Ticks;
            for (var i = 0; i < repeats; ++i)
            {
                DecryptAesBCrypt(encrypted128, key128, iv);
            }
            var μsAesBCrypt = ((DateTime.UtcNow.Ticks - startTicks) / 10.0) / repeats;
            Console.WriteLine($"DecryptAesBCrypt: {μsAesBCrypt:.00} μs, ratio=100%");

            // measure DecryptAesDotNetTransform
            startTicks = DateTime.UtcNow.Ticks;
            for (var i = 0; i < repeats; ++i)
            {
                DecryptAesDotNetTransform(encrypted128, key128, iv);
            }
            var μsAesDotNetTransform = ((DateTime.UtcNow.Ticks - startTicks) / 10.0) / repeats;
            Console.WriteLine($"DecryptAesDotNetTransform: {μsAesDotNetTransform:.00} μs, ratio={100 * μsAesDotNetTransform / μsAesBCrypt:.}%");

            // measure DecryptAesDotNetStream
            startTicks = DateTime.UtcNow.Ticks;
            for (var i = 0; i < repeats; ++i)
            {
                DecryptAesDotNetStream(encrypted128, key128, iv);
            }
            var μsAesDotNetStream = ((DateTime.UtcNow.Ticks - startTicks) / 10.0) / repeats;
            Console.WriteLine($"DecryptAesDotNetStream: {μsAesDotNetStream:.00} μs, ratio={100 * μsAesDotNetStream/ μsAesBCrypt:.}%");
        }

        public static byte[] DecryptAesDotNetTransform(byte[] encrypted, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                using (var decryptor = aes.CreateDecryptor(key, iv))
                {
                    return decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                }
            }
        }

        public static byte[] DecryptAesDotNetStream(byte[] encrypted, byte[] key, byte[] iv)
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

                    return msDecrypted.ToArray();
                }
            }
        }

        private static readonly IntPtr AesAlgHandle = Marshal.StringToHGlobalUni("AES");
        private static readonly IntPtr ChainingMode = Marshal.StringToHGlobalUni("ChainingMode");
        private static readonly IntPtr ChainingModeCbc = Marshal.StringToHGlobalUni("ChainingModeCBC");
        private const uint ChainingModeCbcLength = 32; // ChainingModeCBC.Length * 2

        public static byte[] DecryptAesBCrypt(byte[] encrypted, byte[] key, byte[] iv)
        {
            iv.CopyTo(ivCopy, 0);
            return DecryptAes128(encrypted, 0, (uint)encrypted.Length, key, ivCopy);
        }

        private static byte[] DecryptAes128(byte[] encrypted, int index, uint size, byte[] key, byte[] iv)
        {
            IntPtr keyHandle = IntPtr.Zero;
            byte[] decrypted = null;

            long ntStatus = BCryptOpenAlgorithmProvider(out var aesAlgHandle, AesAlgHandle, IntPtr.Zero, 0);
            if (ntStatus < 0)
            {
                goto cleanup;
            }

            unsafe
            {
                fixed (byte* keyBytes = &key[0])
                {
                    ntStatus = BCryptGenerateSymmetricKey(aesAlgHandle, out keyHandle, IntPtr.Zero, 0, keyBytes, key.Length, 0);
                    if (ntStatus < 0)
                    {
                        goto cleanup;
                    }
                }
            }

            ntStatus = BCryptSetProperty(keyHandle, ChainingMode, ChainingModeCbc, ChainingModeCbcLength, 0);
            if (ntStatus < 0)
            {
                goto cleanup;
            }

            unsafe
            {
                fixed (byte* cipherText = &encrypted[index])
                {
                    fixed (byte* initVector = &iv[0])
                    {
                        uint flags = 1;
                        ntStatus = BCryptDecrypt(keyHandle, cipherText, size, IntPtr.Zero, initVector, (uint)iv.Length, (byte*)0, 0, out var resultLength, flags);
                        if (ntStatus < 0)
                        {
                            goto cleanup;
                        }

                        decrypted = new byte[resultLength];

                        fixed (byte* output = &decrypted[0])
                        {
                            ntStatus = BCryptDecrypt(keyHandle, cipherText, size, IntPtr.Zero, initVector, (uint)iv.Length, output, (uint)decrypted.Length, out resultLength, flags);
                            if (ntStatus < 0)
                            {
                                goto cleanup;
                            }

                            var tmp = new byte[resultLength];
                            Array.Copy(decrypted, 0, tmp, 0, resultLength);
                            decrypted = tmp;
                        }
                    }
                }
            }

        cleanup:
            if (keyHandle != IntPtr.Zero)
            {
                BCryptDestroyKey(keyHandle);
            }

            if (aesAlgHandle != IntPtr.Zero)
            {
                BCryptCloseAlgorithmProvider(aesAlgHandle, 0);
            }

            return decrypted;
        }

        [DllImport("bcrypt.dll")]
        private static extern int BCryptOpenAlgorithmProvider(out IntPtr aesAlgHandle, IntPtr algName, IntPtr implementation, uint flags);

        [DllImport("bcrypt.dll")]
        private static extern unsafe int BCryptGenerateSymmetricKey(IntPtr aesAlgHandle, out IntPtr keyHandle, IntPtr keyObject, uint keyObjectSize, byte* keyBytes, int keyBytesSize, uint flags);

        [DllImport("bcrypt.dll")]
        private static extern int BCryptSetProperty(IntPtr keyHandle, IntPtr property, IntPtr input, uint inputLength, uint flags);

        [DllImport("bcrypt.dll")]
        private static extern unsafe int BCryptDecrypt(IntPtr keyHandle, byte* cipherText, uint cipherTextLength, IntPtr paddingInfo, byte* initVector, uint initVectorLength, byte* output, uint outputLength, out uint resultLength, uint flags);

        [DllImport("bcrypt.dll")]
        private static extern int BCryptDestroyKey(IntPtr keyHandle);

        [DllImport("bcrypt.dll")]
        private static extern int BCryptCloseAlgorithmProvider(IntPtr aesAlgHandle, uint flags);

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
