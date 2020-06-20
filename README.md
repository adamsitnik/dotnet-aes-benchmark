# dotnet-aes-benchmark
Benchmark of AES decryption

# Results
## Windows Host benchmark_simple
```
dotnet benchmark_simple.dll
Warm up
Measure
DecryptAesBCrypt: 4.48 µs, ratio=100%
DecryptAesDotNetTransform: 5.74 µs, ratio=128%
DecryptAesDotNetStream: 9.32 µs, ratio=208%
```

## Windows Host
```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.329 (2004/?/20H1)
Intel Core i7-6700 CPU 3.40GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.4.20258.7
  [Host]        : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.25106, CoreFX 5.0.20.25106), X64 RyuJIT
```
  
|                 Method |       Runtime |       Mean |    Error |    StdDev | Ratio |
|----------------------- |-------------- |-----------:|---------:|----------:|------:|
|    AesDecryptDotNet128 | .NET Core 3.1 | 4,660.7 ns | 91.93 ns | 125.84 ns |  1.00 |
|    AesDecryptDotNet128 | .NET Core 5.0 | 4,420.3 ns | 56.67 ns |  53.01 ns |  0.96 |
|                        |               |            |          |           |       |
|    AesDecryptDotNet192 | .NET Core 3.1 | 4,491.4 ns | 42.66 ns |  39.91 ns |  1.00 |
|    AesDecryptDotNet192 | .NET Core 5.0 | 4,414.1 ns | 54.31 ns |  45.35 ns |  0.98 |
|                        |               |            |          |           |       |
|    AesDecryptDotNet256 | .NET Core 3.1 | 4,511.6 ns | 66.19 ns |  58.67 ns |  1.00 |
|    AesDecryptDotNet256 | .NET Core 5.0 | 4,377.8 ns | 37.17 ns |  34.77 ns |  0.97 |
|                        |               |            |          |           |       |
| AesDecryptBCryptWin128 | .NET Core 3.1 | 1,028.7 ns | 20.05 ns |  31.22 ns |  1.00 |
| AesDecryptBCryptWin128 | .NET Core 5.0 |   996.8 ns | 19.83 ns |  21.21 ns |  0.96 |
|                        |               |            |          |           |       |
| AesDecryptBCryptWin192 | .NET Core 3.1 | 1,024.5 ns | 18.72 ns |  16.60 ns |  1.00 |
| AesDecryptBCryptWin192 | .NET Core 5.0 | 1,011.1 ns | 19.32 ns |  18.08 ns |  0.99 |
|                        |               |            |          |           |       |
| AesDecryptBCryptWin256 | .NET Core 3.1 | 1,017.7 ns | 17.79 ns |  15.77 ns |  1.00 |
| AesDecryptBCryptWin256 | .NET Core 5.0 | 1,027.7 ns | 20.37 ns |  35.13 ns |  1.01 |
|                        |               |            |          |           |       |
|   AesDecryptOpenSsl128 | .NET Core 3.1 |   746.2 ns | 12.39 ns |  10.34 ns |  1.00 |
|   AesDecryptOpenSsl128 | .NET Core 5.0 |   706.3 ns |  6.21 ns |   5.50 ns |  0.95 |
|                        |               |            |          |           |       |
|   AesDecryptOpenSsl192 | .NET Core 3.1 |   756.2 ns | 14.89 ns |  13.93 ns |  1.00 |
|   AesDecryptOpenSsl192 | .NET Core 5.0 |   729.9 ns | 14.04 ns |  15.02 ns |  0.97 |
|                        |               |            |          |           |       |
|   AesDecryptOpenSsl256 | .NET Core 3.1 |   749.9 ns |  9.54 ns |   8.92 ns |  1.00 |
|   AesDecryptOpenSsl256 | .NET Core 5.0 |   729.7 ns | 13.92 ns |  13.67 ns |  0.97 |

## WSL2 on Windows Host
```
BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04 (WSL)
Intel Core i7-6700 CPU 3.40GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.5.20279.10
  [Host]        : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.27801, CoreFX 5.0.20.27801), X64 RyuJIT
```

|                 Method |       Runtime |        Mean |    Error |    StdDev | Ratio |
|----------------------- |-------------- |------------:|---------:|----------:|------:|
|    AesDecryptDotNet128 | .NET Core 3.1 | 4,949.26 ns | 37.68 ns |  35.24 ns |  1.00 |
|    AesDecryptDotNet128 | .NET Core 5.0 | 5,085.84 ns | 80.45 ns |  71.32 ns |  1.03 |
|                        |               |             |          |           |       |
|    AesDecryptDotNet192 | .NET Core 3.1 | 5,164.30 ns | 99.04 ns | 181.10 ns |  1.00 |
|    AesDecryptDotNet192 | .NET Core 5.0 | 5,042.20 ns | 47.23 ns |  39.44 ns |  0.94 |
|                        |               |             |          |           |       |
|    AesDecryptDotNet256 | .NET Core 3.1 | 5,114.30 ns | 57.48 ns |  53.76 ns |  1.00 |
|    AesDecryptDotNet256 | .NET Core 5.0 | 5,137.80 ns | 65.50 ns |  61.27 ns |  1.00 |
|                        |               |             |          |           |       |
|   AesDecryptOpenSsl128 | .NET Core 3.1 |   581.92 ns |  6.03 ns |   5.34 ns |  1.00 |
|   AesDecryptOpenSsl128 | .NET Core 5.0 |   519.49 ns |  8.04 ns |   7.52 ns |  0.89 |
|                        |               |             |          |           |       |
|   AesDecryptOpenSsl192 | .NET Core 3.1 |   608.58 ns |  7.77 ns |   6.07 ns |  1.00 |
|   AesDecryptOpenSsl192 | .NET Core 5.0 |   530.70 ns |  9.90 ns |   8.27 ns |  0.87 |
|                        |               |             |          |           |       |
|   AesDecryptOpenSsl256 | .NET Core 3.1 |   596.65 ns |  9.55 ns |   7.98 ns |  1.00 |
|   AesDecryptOpenSsl256 | .NET Core 5.0 |   536.25 ns | 10.04 ns |  10.31 ns |  0.90 |
