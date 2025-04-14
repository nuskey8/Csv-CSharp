using System.Buffers;
using System.Text;

namespace Csv.Tests;

public class CsvWriterTests
{
    [Test]
    public void Test_WriteRaw()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();

        var csvWriter = new CsvWriter(bufferWriter, new());
        csvWriter.WriteRaw("c1"u8);
        csvWriter.WriteSeparator();
        csvWriter.WriteRaw("c2"u8);
        csvWriter.WriteSeparator();
        csvWriter.WriteRaw("c3"u8);
        csvWriter.WriteEndOfLine();
        csvWriter.WriteRaw("123"u8);
        csvWriter.WriteSeparator();
        csvWriter.WriteRaw("456"u8);
        csvWriter.WriteSeparator();
        csvWriter.WriteRaw("789"u8);

        Assert.That(Encoding.UTF8.GetString(bufferWriter.WrittenSpan), Is.EqualTo("c1,c2,c3\n123,456,789"));
    }

    [Test]
    public void Test_WriteInt16()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();

        var csvWriter = new CsvWriter(bufferWriter, new());
        csvWriter.WriteRaw("c1"u8);
        csvWriter.WriteSeparator();
        csvWriter.WriteRaw("c2"u8);
        csvWriter.WriteSeparator();
        csvWriter.WriteRaw("c3"u8);
        csvWriter.WriteEndOfLine();
        csvWriter.WriteInt32(123);
        csvWriter.WriteSeparator();
        csvWriter.WriteInt32(456);
        csvWriter.WriteSeparator();
        csvWriter.WriteInt32(789);

        Assert.That(Encoding.UTF8.GetString(bufferWriter.WrittenSpan), Is.EqualTo("c1,c2,c3\n123,456,789"));
    }

    [Test]
    public void Test_WriteUtf16()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();

        var csvWriter = new CsvWriter(bufferWriter, new()
        {
            QuoteMode = QuoteMode.All
        });
        csvWriter.WriteString("c1");
        csvWriter.WriteSeparator();
        csvWriter.WriteString("c2");
        csvWriter.WriteSeparator();
        csvWriter.WriteString("c3");
        csvWriter.WriteEndOfLine();
        csvWriter.WriteString("foo");
        csvWriter.WriteSeparator();
        csvWriter.WriteString("bar");
        csvWriter.WriteSeparator();
        csvWriter.WriteString("baz");

        Assert.That(Encoding.UTF8.GetString(bufferWriter.WrittenSpan), Is.EqualTo(
@"""c1"",""c2"",""c3""
""foo"",""bar"",""baz"""
        ));
    }


    [Test]
    public void Test_WriteUtf16_Escape()
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var csvWriter = new CsvWriter(bufferWriter, new());
        csvWriter.WriteString("f\"oo");
        Assert.That(Encoding.UTF8.GetString(bufferWriter.WrittenSpan), Is.EqualTo(@"""f""""oo"""));
    }

    [Test]
    [TestCase(true, "true", QuoteMode.None)]
    [TestCase(false, "false", QuoteMode.None)]
    [TestCase(true, "\"true\"", QuoteMode.All)]
    [TestCase(false, "\"false\"", QuoteMode.All)]
    public void Test_WriteBoolean(bool b, string str, QuoteMode quoteMode)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var csvWriter = new CsvWriter(bufferWriter, new() { QuoteMode = quoteMode });

        csvWriter.WriteBoolean(b);
        Assert.That(Encoding.UTF8.GetString(bufferWriter.WrittenSpan), Is.EqualTo(str));
    }
}