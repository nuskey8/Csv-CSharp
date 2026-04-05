using BenchmarkDotNet.Attributes;
using Csv;
using CsvHelper.Configuration;
using nietras.SeparatedValues;
using System.Globalization;

[Config(typeof(BenchmarkConfig))]
public class DeserializeFileStream
{
    const string Path = "file.csv";

    CsvConfiguration config = default!;
    FileStream fs = default!;

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

        File.WriteAllBytes(Path, CsvData.Utf8Text1);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        fs = new FileStream(Path, FileMode.Open);
    }

    [Benchmark(Description = "Csv-CSharp")]
    public Person[] CsvCSharp()
    {
        var result = new Person[100];
        CsvSerializer.Deserialize<Person>(fs, result);
        return result;
    }

    [Benchmark(Description = "ServiceStack.Text")]
    public List<Person> ServiceStackText()
    {
        return ServiceStack.Text.CsvSerializer.DeserializeFromStream<List<Person>>(fs);
    }

    [Benchmark(Description = "CsvHelper")]
    public Person[] CsvHelper()
    {
        using var reader = new CsvHelper.CsvReader(new StreamReader(fs), config);
        return reader!.GetRecords<Person>().ToArray();
    }

    [Benchmark(Description = "Sep")]
    public Person[] Deserialize_Sep()
    {
        using var reader = Sep.Reader().From(fs);
        var result = new Person[100];

        var index = 0;
        foreach (var row in reader)
        {
            var item = new Person
            {
                Name = row["Name"].Parse<string>(),
                Age = row["Age"].Parse<int>(),
            };
            result[index] = item;
            index++;
        }

        return result;
    }
}