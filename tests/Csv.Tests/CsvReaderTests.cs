using System.Text;

namespace Csv.Tests;

public class CsvReaderTests
{
    [Test]
    public void Test_ReadInt32()
    {
        var x = @"""100"""u8;
        var reader = new CsvReader(new(x.ToArray()), new());

        var actual = reader.ReadInt32();
        Assert.That(actual, Is.EqualTo(100));
    }

    [Test]
    [TestCase("true")]
    [TestCase("True")]
    public void Test_ReadBoolean_True(string str)
    {
        var reader = new CsvReader(new(Encoding.UTF8.GetBytes(str)), new());

        var actual = reader.ReadBoolean();
        Assert.That(actual, Is.EqualTo(true));
    }

    [Test]
    [TestCase("false")]
    [TestCase("False")]
    public void Test_ReadBoolean_False(string str)
    {
        var reader = new CsvReader(new(Encoding.UTF8.GetBytes(str)), new());

        var actual = reader.ReadBoolean();
        Assert.That(actual, Is.EqualTo(false));
    }

    [Test]
    public void Test_ReadString()
    {
        var x = @"foo"u8;
        var reader = new CsvReader(new(x.ToArray()), new());

        var actual = reader.ReadString();
        Assert.That(actual, Is.EqualTo("foo"));
    }

    [Test]
    public void Test_ReadString_WithDoubleQuotation()
    {
        var x = @"""f,oo"""u8;
        var reader = new CsvReader(new(x.ToArray()), new());

        var actual = reader.ReadString();
        Assert.That(actual, Is.EqualTo("f,oo"));
    }

    [Test]
    public void Test_ReadString_WithEscape()
    {
        var x = @"""f""""oo"""u8;
        var reader = new CsvReader(new(x.ToArray()), new());

        var actual = reader.ReadString();
        Assert.That(actual, Is.EqualTo("f\"oo"));
    }

    [Test]
    public void Test_ReadString_Null()
    {
        var x = @""u8;
        var reader = new CsvReader(new(x.ToArray()), new());

        var actual = reader.ReadString();
        Assert.That(actual, Is.EqualTo(null));
    }

    [Test]
    public void Test_ReadString_Empty()
    {
        var x = @""""""u8;
        var reader = new CsvReader(new(x.ToArray()), new());

        var actual = reader.ReadString();
        Assert.That(actual, Is.EqualTo(""));
    }
}