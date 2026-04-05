using BenchmarkDotNet.Attributes;
using Csv;
using CsvHelper.Configuration;
using nietras.SeparatedValues;
using System.Globalization;

[Config(typeof(BenchmarkConfig))]
public class Deserialize2
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
    public Numbers[] Deserialize_CsvCSharp()
    {
        var result = new Numbers[100];
        CsvSerializer.Deserialize<Numbers>(CsvData.Utf8Text2, result);
        return result;
    }

    [Benchmark(Description = "ServiceStack.Text")]
    public List<Numbers> Deserialize_ServiceStackText()
    {
        return ServiceStack.Text.CsvSerializer.DeserializeFromString<List<Numbers>>(CsvData.Text2);
    }

    [Benchmark(Description = "CsvHelper")]
    public Numbers[] Deserialize_CsvHelper()
    {
        using var reader = new CsvHelper.CsvReader(new StringReader(CsvData.Text2), config);
        return reader.GetRecords<Numbers>().ToArray();
    }

    [Benchmark(Description = "Sep")]
    public Numbers[] Deserialize_Sep()
    {
        using var reader = Sep.Reader().From(CsvData.Utf8Text2);
        var result = new Numbers[100];

        var index = 0;
        foreach (var row in reader)
        {
            var item = new Numbers
            {
                Alpha = row["Alpha"].Parse<int>(),
                Beta = row["Beta"].Parse<int>(),
                Gamma = row["Gamma"].Parse<int>(),
            };
            result[index] = item;
            index++;
        }

        return result;
    }
}