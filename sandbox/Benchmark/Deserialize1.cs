using BenchmarkDotNet.Attributes;
using Csv;
using CsvHelper.Configuration;
using nietras.SeparatedValues;
using System.Globalization;

[Config(typeof(BenchmarkConfig))]
public class Deserialize1
{
    CsvConfiguration config = default!;

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

    [Benchmark(Description = "Csv-CSharp")]
    public Person[] Deserialize_CsvCSharp()
    {
        var result = new Person[100];
        CsvSerializer.Deserialize<Person>(CsvData.Utf8Text1, result);
        return result;
    }

    [Benchmark(Description = "ServiceStack.Text")]
    public List<Person> Deserialize_ServiceStackText()
    {
        return ServiceStack.Text.CsvSerializer.DeserializeFromString<List<Person>>(CsvData.Text1);
    }

    [Benchmark(Description = "CsvHelper")]
    public Person[] Deserialize_CsvHelper()
    {
        using var reader = new CsvHelper.CsvReader(new StringReader(CsvData.Text1), config);
        return reader.GetRecords<Person>().ToArray();
    }

    [Benchmark(Description = "Sep")]
    public Person[] Deserialize_Sep()
    {
        using var reader = Sep.Reader().From(CsvData.Utf8Text1);
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