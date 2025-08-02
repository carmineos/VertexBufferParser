using BenchmarkDotNet.Running;
using VertexBufferParser.Benchmarks;

BenchmarkRunner.Run<ParserBenchmarks>();
BenchmarkRunner.Run<WriterBenchmarks>();
BenchmarkRunner.Run<PositionOffsetBenchmarks>();
