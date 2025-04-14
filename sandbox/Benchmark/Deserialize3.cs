using BenchmarkDotNet.Attributes;
using Csv;
using CsvHelper.Configuration;
using nietras.SeparatedValues;
using ServiceStack;
using System.Globalization;

[Config(typeof(BenchmarkConfig))]
public class Deserialize3
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
    public NumbersStringKey[] Deserialize_CsvCSharp()
    {
        var result = new NumbersStringKey[100];
        CsvSerializer.Deserialize<NumbersStringKey>(CsvData.Utf8Text2, result);
        return result;
    }

    [Benchmark(Description = "ServiceStack.Text")]
    public List<NumbersStringKey> Deserialize_ServiceStackText()
    {
        return ServiceStack.Text.CsvSerializer.DeserializeFromString<List<NumbersStringKey>>(CsvData.Text2);
    }

    [Benchmark(Description = "CsvHelper")]
    public NumbersStringKey[] Deserialize_CsvHelper()
    {
        using var reader = new CsvHelper.CsvReader(new StringReader(CsvData.Text2), config);
        return reader.GetRecords<NumbersStringKey>().ToArray();
    }

    [Benchmark(Description = "Sep")]
    public NumbersStringKey[] Deserialize_Sep()
    {
        using var reader = Sep.Reader().From(CsvData.Utf8Text2);
        var result = new NumbersStringKey[100];

        var index = 0;
        foreach (var row in reader)
        {
            var item = new NumbersStringKey
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