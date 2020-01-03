# 2018-12-17 Before object context removal

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
|                Empty |    64.02 ns | 0.4003 ns | 0.3744 ns |  1.00 |    0.00 |      0.0228 |           - |           - |                72 B |
|        SingleElement |    71.12 ns | 0.2857 ns | 0.2673 ns |  1.11 |    0.01 |      0.0228 |           - |           - |                72 B |
|         EmptyAutofac | 1,003.64 ns | 3.5256 ns | 3.2978 ns | 15.68 |    0.08 |      0.4368 |           - |           - |              1376 B |
| SingleElementAutofac | 1,386.19 ns | 4.6067 ns | 4.3091 ns | 21.65 |    0.15 |      0.5856 |           - |           - |              1848 B |

## InProcCQRS__Commands

|                                      Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|              CommandWithoutInlineObjContext |  4.401 us | 0.0528 us | 0.0494 us |  1.00 |    0.00 |      1.2512 |           - |           - |             3.85 KB |
|             PlainCommandWithSecuredPipeline |  9.079 us | 0.1130 us | 0.0944 us |  2.06 |    0.03 |      1.8616 |           - |           - |             5.73 KB |
|           SecuredCommandWithSecuredPipeline | 16.692 us | 0.2616 us | 0.2569 us |  3.79 |    0.08 |      2.4414 |           - |           - |             7.57 KB |
| SecuredButFailingCommandWithSecuredPipeline | 16.879 us | 0.0895 us | 0.0699 us |  3.83 |    0.04 |      2.4719 |           - |           - |             7.64 KB |
|           PlainCommandWithValidatedPipeline |  8.300 us | 0.1039 us | 0.0921 us |  1.89 |    0.03 |      2.1667 |           - |           - |              6.7 KB |
|           ValidCommandWithValidatedPipeline | 11.514 us | 0.2253 us | 0.2108 us |  2.62 |    0.05 |      2.8687 |           - |           - |             8.84 KB |
|         InvalidCommandWithValidatedPipeline | 13.030 us | 0.1787 us | 0.1672 us |  2.96 |    0.04 |      2.5787 |           - |           - |             7.94 KB |

## InProcCQRS__Queries

|                                    Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------------------------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|              QueryWithoutInlineObjContext |  4.398 us | 0.0465 us | 0.0435 us |  1.00 |    0.00 |      1.2894 |           - |           - |             3.97 KB |
|              PlainQueryWithCachedPipeline |  9.840 us | 0.2330 us | 0.2392 us |  2.24 |    0.05 |      1.2970 |           - |           - |             4.02 KB |
|             CachedQueryWithCachedPipeline |  9.697 us | 0.0645 us | 0.0538 us |  2.20 |    0.03 |      1.2970 |           - |           - |             4.02 KB |
|             PlainQueryWithSecuredPipeline |  9.247 us | 0.2247 us | 0.3150 us |  2.13 |    0.08 |      1.8921 |           - |           - |             5.85 KB |
|           SecuredQueryWithSecuredPipeline | 17.138 us | 0.1004 us | 0.0890 us |  3.90 |    0.05 |      2.4719 |           - |           - |             7.69 KB |
| SecuredQueryButFailingWithSecuredPipeline | 90.416 us | 0.4015 us | 0.3756 us | 20.56 |    0.23 |      2.8076 |           - |           - |             8.67 KB |

## RemoteCQRS

|                Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|      SimpleMiddleware |  5.776 us | 0.0250 us | 0.0234 us |  1.00 |    0.00 |      4.3488 |           - |           - |            13.38 KB |
|            EmptyQuery | 18.753 us | 0.1874 us | 0.1661 us |  3.25 |    0.03 |      5.7678 |           - |           - |            17.82 KB |
|          EmptyCommand | 20.617 us | 0.0795 us | 0.0705 us |  3.57 |    0.02 |      5.7983 |           - |           - |            17.85 KB |
|   MultipleFieldsQuery | 23.077 us | 0.1055 us | 0.0935 us |  4.00 |    0.02 |      5.8594 |           - |           - |            18.03 KB |
| MultipleFieldsCommand | 22.930 us | 0.2724 us | 0.2415 us |  3.97 |    0.05 |      5.8289 |           - |           - |            17.95 KB |
