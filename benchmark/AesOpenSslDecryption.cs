namespace benchmark
{
    using System;
    using System.Runtime.InteropServices;

    public static class AesOpenSslDecryption
    {
        private static readonly IntPtr AesAlgHandle = Marshal.StringToHGlobalUni("AES");
        private static readonly IntPtr ChainingMode = Marshal.StringToHGlobalUni("ChainingMode");
        private static readonly IntPtr ChainingModeCbc = Marshal.StringToHGlobalUni("ChainingModeCBC");
        private const uint ChainingModeCbcLength = 32; // ChainingModeCBC.Length * 2

        public static long DecryptAes(byte[] encrypted, byte[] key, byte[] iv, out byte[] decrypted)
        {
            return DecryptAes128(encrypted, 0, (uint)encrypted.Length, key, iv, out decrypted);
        }

        private static long DecryptAes128(byte[] encrypted, int index, uint size, byte[] key, byte[] iv, out byte[] decrypted)
        {
            IntPtr keyHandle = IntPtr.Zero;
            decrypted = null;

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

            return ntStatus;
        }

        [DllImport("bcrypt2.dll")]
        private static extern int BCryptOpenAlgorithmProvider(out IntPtr aesAlgHandle, IntPtr algName, IntPtr implementation, uint flags);

        [DllImport("bcrypt2.dll")]
        private static extern unsafe int BCryptGenerateSymmetricKey(IntPtr aesAlgHandle, out IntPtr keyHandle, IntPtr keyObject, uint keyObjectSize, byte* keyBytes, int keyBytesSize, uint flags);

        [DllImport("bcrypt2.dll")]
        private static extern int BCryptSetProperty(IntPtr keyHandle, IntPtr property, IntPtr input, uint inputLength, uint flags);

        [DllImport("bcrypt2.dll")]
        private static extern unsafe int BCryptDecrypt(IntPtr keyHandle, byte* cipherText, uint cipherTextLength, IntPtr paddingInfo, byte* initVector, uint initVectorLength, byte* output, uint outputLength, out uint resultLength, uint flags);

        [DllImport("bcrypt2.dll")]
        private static extern int BCryptDestroyKey(IntPtr keyHandle);

        [DllImport("bcrypt2.dll")]
        private static extern int BCryptCloseAlgorithmProvider(IntPtr aesAlgHandle, uint flags);
    }
}