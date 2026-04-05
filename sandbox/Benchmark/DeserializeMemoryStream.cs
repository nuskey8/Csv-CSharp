using BenchmarkDotNet.Attributes;
using Csv;
using CsvHelper.Configuration;
using nietras.SeparatedValues;
using System.Globalization;

[Config(typeof(BenchmarkConfig))]
public class DeserializeMemoryStream
{
    CsvConfiguration config = default!;
    MemoryStream ms = default!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        CsvSerializer.DefaultOptions = new()
        {
            AllowComments = false,
        };
        config = new(CultureInfo.InvariantCulture)
        {
            AllowComments = false,
        };
    }

    [IterationSetup]
    public void IterationSetup()
    {
        ms = new(CsvData.Utf8Text1);
    }

    [Benchmark(Description = "Csv-CSharp")]
    public Person[] CsvCSharp()
    {
        var result = new Person[100];
        CsvSerializer.Deserialize<Person>(ms, result);
        return result;
    }

    [Benchmark(Description = "ServiceStack.Text")]
    public List<Person> ServiceStackText()
    {
        return ServiceStack.Text.CsvSerializer.DeserializeFromStream<List<Person>>(ms);
    }

    [Benchmark(Description = "CsvHelper")]
    public Person[] CsvHelper()
    {
        using var reader = new CsvHelper.CsvReader(new StreamReader(ms), config);
        return reader.GetRecords<Person>().ToArray();
    }

    [Benchmark(Description = "Sep")]
    public Person[] Deserialize_Sep()
    {
        using var reader = Sep.Reader().From(ms);
        var result = new Person[100];

        var index = 0;
        foreach (var row in reader)
        {
            var item = new Person
            {
                Name = row[0].Parse<string>(),
                Age = row[1].Parse<int>(),
            };
            result[index] = item;
            index++;
        }

        return result;
    }
}