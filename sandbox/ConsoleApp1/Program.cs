using System.Text;
using Csv;
using Csv.Annotations;

Person[] data = [
    new() { Name = "Alice", Age = 18, Gender = Gender.Female },
    new() { Name = "Bob", Age = 21, Gender = Gender.Male },
    new() { Name = "Carol", Age = 33, Gender = Gender.Female },
    new() { Name = "Dave", Age = 27, Gender = Gender.Male }
];

var csv = CsvSerializer.Serialize(data);
Console.WriteLine(Encoding.UTF8.GetString(csv));

var deserialized = CsvSerializer.Deserialize<Person>(csv);
foreach (var item in deserialized)
{
    Console.WriteLine(item);
}

[CsvObject]
public partial record Person
{
    [Column(0)]
    public string? Name { get; init; }

    [Column(1)]
    public int Age { get; init; }

    [Column(2)]
    public Gender Gender { get; init; }
}

public enum Gender
{
    Male,
    Female
}