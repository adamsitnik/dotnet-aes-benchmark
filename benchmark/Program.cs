using BenchmarkDotNet.Running;

namespace benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkAesImpl>();
        }
    }
}
