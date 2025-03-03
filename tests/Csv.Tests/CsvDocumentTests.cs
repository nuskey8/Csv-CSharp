using System.Text;

namespace Csv.Tests;

public class CsvDocumentTests
{
    [Test]
    public void Test_CsvDocument_Index()
    {
        var csv =
@"Name,Age
Alex,21  
Bob,35
Charles,17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("Name"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("Age"));

        Assert.That(document.Rows[0].Length, Is.EqualTo(2));
        Assert.That(document.Rows[0][0].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0][1].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1].Length, Is.EqualTo(2));
        Assert.That(document.Rows[1][0].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1][1].GetValue<int>(), Is.EqualTo(35));

        Assert.That(document.Rows[2].Length, Is.EqualTo(2));
        Assert.That(document.Rows[2][0].GetValue<string>(), Is.EqualTo("Charles"));
        Assert.That(document.Rows[2][1].GetValue<int>(), Is.EqualTo(17));
    }

    [Test]
    public void Test_CsvDocument_Key()
    {
        var csv =
@"Name,Age
Alex,21  
Bob,35
Charles,17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows[0].Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1].Length, Is.EqualTo(2));
        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));

        Assert.That(document.Rows[2].Length, Is.EqualTo(2));
        Assert.That(document.Rows[2]["Name"].GetValue<string>(), Is.EqualTo("Charles"));
        Assert.That(document.Rows[2]["Age"].GetValue<int>(), Is.EqualTo(17));
    }

    [Test]
    public void Test_CsvDocument_CommasAtEoL()
    {
        var csv = "colA,colB,colC,colD\nA1,B1,C1,\nA2,B2,C2,D2"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(4));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("colA"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("colB"));
        Assert.That(document.Header[2].GetValue<string>(), Is.EqualTo("colC"));
        Assert.That(document.Header[3].GetValue<string>(), Is.EqualTo("colD"));

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0].Length, Is.EqualTo(3));
        Assert.That(document.Rows[0][0].GetValue<string>(), Is.EqualTo("A1"));
        Assert.That(document.Rows[0][1].GetValue<string>(), Is.EqualTo("B1"));
        Assert.That(document.Rows[0][2].GetValue<string>(), Is.EqualTo("C1"));

        Assert.That(document.Rows[1].Length, Is.EqualTo(4));
        Assert.That(document.Rows[1][0].GetValue<string>(), Is.EqualTo("A2"));
        Assert.That(document.Rows[1][1].GetValue<string>(), Is.EqualTo("B2"));
        Assert.That(document.Rows[1][2].GetValue<string>(), Is.EqualTo("C2"));
        Assert.That(document.Rows[1][3].GetValue<string>(), Is.EqualTo("D2"));
    }
}