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
|                   Method |       Runtime |     Mean |     Error |    StdDev | Ratio |
|------------------------- |-------------- |---------:|----------:|----------:|------:|
|      AesDecryptDotNet128 | .NET Core 3.1 | 9.925 us | 0.1985 us | 0.4906 us |  1.82 |
| AesDecryptDotNet128Type2 | .NET Core 3.1 | 6.014 us | 0.1185 us | 0.1541 us |  1.03 |
|   AesDecryptBCryptWin128 | .NET Core 3.1 | 4.499 us | 0.0521 us | 0.0462 us |  0.78 |
|     AesDecryptOpenSsl128 | .NET Core 3.1 | 4.463 us | 0.0708 us | 0.0591 us |  0.77 |
|      AesDecryptDotNet128 | .NET Core 5.0 | 9.467 us | 0.0692 us | 0.0613 us |  1.63 |
| AesDecryptDotNet128Type2 | .NET Core 5.0 | 5.804 us | 0.0465 us | 0.0412 us |  1.00 baseline |
|   AesDecryptBCryptWin128 | .NET Core 5.0 | 4.442 us | 0.0352 us | 0.0329 us |  0.77 |
|     AesDecryptOpenSsl128 | .NET Core 5.0 | 4.138 us | 0.0324 us | 0.0271 us |  0.71 |

## WSL2 on Windows Host
```
BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Core i7-6700 CPU 3.40GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.5.20279.10
  [Host]        : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.27801, CoreFX 5.0.20.27801), X64 RyuJIT
```

|                   Method |       Runtime |     Mean |     Error |    StdDev | Ratio |
|------------------------- |-------------- |---------:|----------:|----------:|------:|
|      AesDecryptDotNet128 | .NET Core 3.1 | 9.572 us | 0.1426 us | 0.1585 us | 1.699 |
| AesDecryptDotNet128Type2 | .NET Core 3.1 | 5.336 us | 0.1040 us | 0.1239 us | 0.946 |
|     AesDecryptOpenSsl128 | .NET Core 3.1 | 3.746 us | 0.0644 us | 0.0602 us | 0.669 |
|      AesDecryptDotNet128 | .NET Core 5.0 | 9.922 us | 0.1156 us | 0.1025 us | 1.771 |
| AesDecryptDotNet128Type2 | .NET Core 5.0 | 5.577 us | 0.1040 us | 0.1900 us | 1.000 baseline |
|     AesDecryptOpenSsl128 | .NET Core 5.0 | 3.690 us | 0.0441 us | 0.0368 us | 0.655 |
