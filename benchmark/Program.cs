using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance.AddJob(Job.Default.WithPlatform(BenchmarkDotNet.Environments.Platform.X64).AsDefault());

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
