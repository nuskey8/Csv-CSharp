using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Csv.Annotations;
using System.Reflection;

BenchmarkSwitcher.FromAssembly(Assembly.GetEntryAssembly()!).Run(args);

class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddDiagnoser(MemoryDiagnoser.Default);
        AddJob(Job.ShortRun.WithWarmupCount(10).WithIterationCount(20));
    }
}

[CsvObject]
public partial record Person
{
    [Column(0)]
    public string? Name { get; set; }
    
    [Column(1)]
    public int Age { get; set; }
}

[CsvObject]
public partial record Numbers
{
    [Column(0)]
    public int Alpha { get; set; }
    [Column(1)]
    public int Beta { get; set; }
    [Column(2)]
    public int Gamma { get; set; }
}

[CsvObject(true)]
public partial record NumbersStringKey
{
    public int Alpha { get; set; }
    public int Beta { get; set; }
    public int Gamma { get; set; }
}