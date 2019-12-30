# 2018-12-17 After object context removal

``` ini

BenchmarkDotNet=v0.11.3, OS=arch
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.100
  [Host] : .NET Core 2.2.7 (CoreCLR 4.6.28008.02, CoreFX 4.6.28008.03), 64bit RyuJIT
  Core   : .NET Core 2.2.7 (CoreCLR 4.6.28008.02, CoreFX 4.6.28008.03), 64bit RyuJIT

Job=Core  Runtime=Core

```

## Pipelines

|               Method |        Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------------- |------------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|                Empty |    61.04 ns | 0.2912 ns | 0.2581 ns |  1.00 |    0.00 |      0.0228 |           - |           - |                72 B |
|        SingleElement |    69.85 ns | 0.3217 ns | 0.3009 ns |  1.14 |    0.01 |      0.0229 |           - |           - |                72 B |
|         EmptyAutofac | 1,006.09 ns | 3.8921 ns | 3.4503 ns | 16.48 |    0.08 |      0.4368 |           - |           - |              1376 B |
| SingleElementAutofac | 1,295.23 ns | 4.5842 ns | 4.2881 ns | 21.22 |    0.09 |      0.5856 |           - |           - |              1848 B |

## InProcCQRS__Commands

|                                      Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|              CommandWithoutInlineObjContext |  3.680 us | 0.0130 us | 0.0122 us |  1.00 |    0.00 |      1.0529 |           - |           - |             3.26 KB |
|             PlainCommandWithSecuredPipeline |  8.132 us | 0.0516 us | 0.0457 us |  2.21 |    0.02 |      1.6632 |           - |           - |             5.14 KB |
|           SecuredCommandWithSecuredPipeline | 15.814 us | 0.0779 us | 0.0729 us |  4.30 |    0.02 |      2.2583 |           - |           - |             6.98 KB |
| SecuredButFailingCommandWithSecuredPipeline | 14.972 us | 0.0722 us | 0.0640 us |  4.07 |    0.02 |      2.2888 |           - |           - |             7.05 KB |
|           PlainCommandWithValidatedPipeline |  5.669 us | 0.0372 us | 0.0330 us |  1.54 |    0.01 |      1.6174 |           - |           - |             4.98 KB |
|           ValidCommandWithValidatedPipeline |  6.188 us | 0.0293 us | 0.0274 us |  1.68 |    0.01 |      1.6174 |           - |           - |             4.98 KB |
|         InvalidCommandWithValidatedPipeline |  5.949 us | 0.0346 us | 0.0324 us |  1.62 |    0.01 |      1.6174 |           - |           - |             4.98 KB |

## InProcCQRS__Queries

|                                    Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------------------------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|              QueryWithoutInlineObjContext |  3.940 us | 0.0128 us | 0.0119 us |  1.00 |    0.00 |      1.0910 |           - |           - |             3.38 KB |
|              PlainQueryWithCachedPipeline | 10.066 us | 0.0785 us | 0.0696 us |  2.56 |    0.02 |      1.3123 |           - |           - |             4.06 KB |
|             CachedQueryWithCachedPipeline | 10.212 us | 0.0554 us | 0.0463 us |  2.59 |    0.01 |      1.3123 |           - |           - |             4.06 KB |
|             PlainQueryWithSecuredPipeline |  8.509 us | 0.0638 us | 0.0597 us |  2.16 |    0.02 |      1.7090 |           - |           - |             5.26 KB |
|           SecuredQueryWithSecuredPipeline | 15.963 us | 0.0709 us | 0.0628 us |  4.05 |    0.02 |      2.2888 |           - |           - |             7.09 KB |
| SecuredQueryButFailingWithSecuredPipeline | 89.585 us | 0.4519 us | 0.4227 us | 22.74 |    0.12 |      2.5635 |           - |           - |             8.05 KB |

## RemoteCQRS

|                Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|      SimpleMiddleware |  5.282 us | 0.0193 us | 0.0150 us |  1.00 |    0.00 |      4.3488 |           - |           - |            13.38 KB |
|            EmptyQuery | 17.146 us | 0.0853 us | 0.0798 us |  3.24 |    0.02 |      5.5847 |           - |           - |            17.23 KB |
|          EmptyCommand | 18.963 us | 0.1133 us | 0.1060 us |  3.60 |    0.02 |      5.6152 |           - |           - |            17.26 KB |
|   MultipleFieldsQuery | 21.414 us | 0.0845 us | 0.0706 us |  4.05 |    0.01 |      5.6458 |           - |           - |            17.44 KB |
| MultipleFieldsCommand | 21.557 us | 0.1782 us | 0.1667 us |  4.09 |    0.02 |      5.6458 |           - |           - |            17.35 KB |
