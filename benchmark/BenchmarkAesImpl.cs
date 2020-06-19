namespace benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    /*
    Benchmarks
            | Windows | Linux
    DotNet  |    X    |   X
    bcrypt  |    X    |
    OpenSSL |    X    |   X
    */

    // This is for quick test if benchmark is running at all
    //[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.NetCoreApp31, launchCount: 5, baseline: true)]
    //[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.NetCoreApp50, launchCount: 5)]

    // Actual config which is used for measuring
    [SimpleJob(RuntimeMoniker.NetCoreApp31, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    public class BenchmarkAesImpl
    {
        static readonly byte[] EmptyByteArray = new byte[0];

        private readonly byte[] key128 = new byte[128/8];
        private readonly byte[] key192 = new byte[192/8];
        private readonly byte[] key256 = new byte[256/8];
        private readonly byte[] iv = new byte[16];
        private readonly byte[] ivCopy = new byte[16];

        private const int N = 10000;
        private readonly byte[] data;

        private readonly byte[] encrypted128;
        private readonly byte[] encrypted192;
        private readonly byte[] encrypted256;
        private readonly bool isWindows;

        public BenchmarkAesImpl()
        {
            Random rnd = new Random(42);
            data = new byte[N];
            rnd.NextBytes(data);

            rnd.NextBytes(key128);
            rnd.NextBytes(key192);
            rnd.NextBytes(key256);

            rnd.NextBytes(iv);

            this.encrypted128 = AesDotNetImpl.Encrypt(data, key128, iv);
            this.encrypted192 = AesDotNetImpl.Encrypt(data, key192, iv);
            this.encrypted256 = AesDotNetImpl.Encrypt(data, key256, iv);

            this.isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            Validate();
        }

        [Benchmark]
        public byte[] AesDecryptDotNet128()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesDotNetImpl.DecryptAes(encrypted128, key128, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptDotNet128 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptDotNet192()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesDotNetImpl.DecryptAes(encrypted192, key192, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptDotNet192 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptDotNet256()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesDotNetImpl.DecryptAes(encrypted256, key256, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptDotNet256 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptDotNet128Type2()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesDotNetImpl2.DecryptAes(encrypted128, key128, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptOpenSsl128 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptBCryptWin128()
        {
            if (!isWindows)
            {
                return EmptyByteArray;
            }

            iv.CopyTo(ivCopy, 0);
            var res = AesWinBcryptDecryption.DecryptAes(encrypted128, key128, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptBCryptWin128 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptBCryptWin192()
        {
            if (!isWindows)
            {
                return EmptyByteArray;
            }

            iv.CopyTo(ivCopy, 0);
            var res = AesWinBcryptDecryption.DecryptAes(encrypted192, key192, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptBCryptWin192 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptBCryptWin256()
        {
            if (!isWindows)
            {
                return EmptyByteArray;
            }

            iv.CopyTo(ivCopy, 0);
            var res = AesWinBcryptDecryption.DecryptAes(encrypted256, key256, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptBCryptWin256 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptOpenSsl128()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesOpenSslDecryption.DecryptAes(encrypted128, key128, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptOpenSsl128 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptOpenSsl192()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesOpenSslDecryption.DecryptAes(encrypted192, key192, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptOpenSsl192 failed");
            }

            return decrypted;
        }

        [Benchmark]
        public byte[] AesDecryptOpenSsl256()
        {
            iv.CopyTo(ivCopy, 0);
            var res = AesOpenSslDecryption.DecryptAes(encrypted256, key256, ivCopy, out var decrypted);
            if (res != 0)
            {
                throw new InvalidOperationException("AesDecryptOpenSsl256 failed");
            }

            return decrypted;
        }

        public void Validate()
        {
            if (!Enumerable.SequenceEqual(data, this.AesDecryptDotNet128())) throw new Exception();
            if (!Enumerable.SequenceEqual(data, this.AesDecryptDotNet192())) throw new Exception();
            if (!Enumerable.SequenceEqual(data, this.AesDecryptDotNet256())) throw new Exception();
            if (!Enumerable.SequenceEqual(data, this.AesDecryptOpenSsl128())) throw new Exception();
            if (!Enumerable.SequenceEqual(data, this.AesDecryptOpenSsl192())) throw new Exception();
            if (!Enumerable.SequenceEqual(data, this.AesDecryptOpenSsl256())) throw new Exception();

            if (this.isWindows)
            {
                if (!Enumerable.SequenceEqual(data, this.AesDecryptBCryptWin128())) throw new Exception();
                if (!Enumerable.SequenceEqual(data, this.AesDecryptBCryptWin192())) throw new Exception();
                if (!Enumerable.SequenceEqual(data, this.AesDecryptBCryptWin256())) throw new Exception();
            }
        }
    }
}
