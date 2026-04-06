using System.Buffers;

namespace Csv.Tests;

public class CsvDocumentTests
{
    // BasicAccess

    [Test]
    public void Test_CsvDocument_BasicAccess_Index()
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
    public void Test_CsvDocument_BasicAccess_Key()
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
    public void Test_CsvDocument_BasicAccess_Unicode()
    {
        var csv =
            @"이름,😊
สมชาย,😀
محمد,😒
明华,😎"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows[0].Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["이름"].GetValue<string>(), Is.EqualTo("สมชาย"));
        Assert.That(document.Rows[0]["😊"].GetValue<string>(), Is.EqualTo("😀"));

        Assert.That(document.Rows[1].Length, Is.EqualTo(2));
        Assert.That(document.Rows[1]["이름"].GetValue<string>(), Is.EqualTo("محمد"));
        Assert.That(document.Rows[1]["😊"].GetValue<string>(), Is.EqualTo("😒"));

        Assert.That(document.Rows[2].Length, Is.EqualTo(2));
        Assert.That(document.Rows[2]["이름"].GetValue<string>(), Is.EqualTo("明华"));
        Assert.That(document.Rows[2]["😊"].GetValue<string>(), Is.EqualTo("😎"));
    }

    [Test]
    public void Test_CsvDocument_BasicAccess_ReadOnlySequence()
    {
        var csv =
@"Name,Age
Alex,21
Bob,35
Charles,17"u8;

        var sequence = new ReadOnlySequence<byte>(csv.ToArray());
        var document = CsvSerializer.ConvertToDocument(sequence);

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Rows.Length, Is.EqualTo(3));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));

        Assert.That(document.Rows[2]["Name"].GetValue<string>(), Is.EqualTo("Charles"));
        Assert.That(document.Rows[2]["Age"].GetValue<int>(), Is.EqualTo(17));
    }

    [Test]
    public void Test_CsvDocument_BasicAccess_ReadOnlySequence_MultiSegment_SplitInField()
    {
        var csv = "Name,Age\nAlice,21\nBob,35"u8.ToArray();

        // Split in the middle of "Alice"
        var first = new MemorySegment<byte>(csv.AsMemory(0, 11));
        var last = first.Append(csv.AsMemory(11));
        var sequence = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);

        var document = CsvSerializer.ConvertToDocument(sequence);

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    [Test]
    public void Test_CsvDocument_BasicAccess_ReadOnlySequence_MultiSegment_SplitAtRowBoundary()
    {
        var csv = "Name,Age\nAlice,21\nBob,35"u8.ToArray();

        // Split at the newline between rows
        var first = new MemorySegment<byte>(csv.AsMemory(0, 17));
        var last = first.Append(csv.AsMemory(17));
        var sequence = new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);

        var document = CsvSerializer.ConvertToDocument(sequence);

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    // Options

    [Test]
    public void Test_CsvDocument_Options_NoHeader()
    {
        var csv =
@"Alex,21
Bob,35
Charles,17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            HasHeader = false,
        });

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
    public void Test_CsvDocument_Options_Separator_Pipe()
    {
        var csv =
            @"Name|Age
Alex|21
Bob|35
Charles|17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            Separator = SeparatorType.Pipe,
        });

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
    public void Test_CsvDocument_Options_Separator_Semicolon()
    {
        var csv =
@"Name;Age
Alex;21
Bob;35
Charles;17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            Separator = SeparatorType.Semicolon,
        });

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
    public void Test_CsvDocument_Options_Separator_Tab()
    {
        var csv =
@"Name	Age
Alex	21
Bob	35
Charles	17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            Separator = SeparatorType.Tab,
        });

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
    public void Test_CsvDocument_Options_NoAllowComments()
    {
        var csv =
@"Name,Age
#Alex,21
Bob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            AllowComments = false,
        });

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("#Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    // Quoting

    [Test]
    public void Test_CsvDocument_Quoting_QuotedKey()
    {
        var csv = "\"Na,me\",\"A\"\"ge\"\nAlice,21\nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("Na,me"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("A\"ge"));

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Na,me"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["A\"ge"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1]["Na,me"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["A\"ge"].GetValue<int>(), Is.EqualTo(35));
    }

    [Test]
    public void Test_CsvDocument_Quoting_QuotedField()
    {
        var csv = "Name,Description\nAlice,\"Hello, World!\"\nBob,\"She said \"\"Hi\"\"\""u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Description"].GetValue<string>(), Is.EqualTo("Hello, World!"));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Description"].GetValue<string>(), Is.EqualTo("She said \"Hi\""));
    }

    [Test]
    public void Test_CsvDocument_Quoting_QuotedField_WithNewline()
    {
        var csv = "Name,Description\nAlice,\"line1\nline2\"\nBob,\"line3\nline4\""u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Description"].GetValue<string>(), Is.EqualTo("line1\nline2"));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Description"].GetValue<string>(), Is.EqualTo("line3\nline4"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_QuotedField_WithCRLF()
    {
        var csv = "Name,Description\nAlice,\"line1\r\nline2\"\nBob,\"line3\r\nline4\""u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Description"].GetValue<string>(), Is.EqualTo("line1\r\nline2"));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Description"].GetValue<string>(), Is.EqualTo("line3\r\nline4"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_QuotedField_WithCR()
    {
        var csv = "Name,Description\nAlice,\"line1\rline2\"\nBob,\"line3\rline4\""u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Description"].GetValue<string>(), Is.EqualTo("line1\rline2"));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Description"].GetValue<string>(), Is.EqualTo("line3\rline4"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_WhitespaceAroundQuotedField()
    {
        // RFC 4180 does not define behavior for whitespace around quoted fields.
        // Leading whitespace before the opening quote is consumed, but trailing
        // whitespace after the closing quote is not - the separator is not found,
        // so the row breaks at the unexpected position.
        var csv = "Name,Age\n  \"Alice\"  ,21\nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(3));
        Assert.That(document.Rows[0].Length, Is.EqualTo(1));
        Assert.That(document.Rows[0][0].GetValue<string>(), Is.EqualTo("Alice"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_LeadingWhitespaceBeforeQuotedField()
    {
        // Leading whitespace before the opening quote is consumed and the field is parsed as quoted
        var csv = "Name,Age\n  \"Alice\",21\nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
    }

    [Test]
    public void Test_CsvDocument_Quoting_TrailingWhitespaceAfterQuotedField()
    {
        // Trailing whitespace after the closing quote prevents the separator from
        // being found, so the remainder of the line is not split correctly.
        var csv = "Name,Age\n\"Alice\"  ,21\nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(3));
        Assert.That(document.Rows[0].Length, Is.EqualTo(1));
        Assert.That(document.Rows[0][0].GetValue<string>(), Is.EqualTo("Alice"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_EmptyQuotedString_IsNotNull()
    {
        var csv = "a,b,c\n,\"\",value"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(1));
        Assert.That(document.Rows[0]["a"].GetValue<string>(), Is.Null);
        Assert.That(document.Rows[0]["b"].GetValue<string>(), Is.EqualTo(""));
        Assert.That(document.Rows[0]["c"].GetValue<string>(), Is.EqualTo("value"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_Separator_Pipe_InsideQuotedField()
    {
        var csv = "Name|Description\nAlice|\"val|ue\"\nBob|normal"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            Separator = SeparatorType.Pipe,
        });

        Assert.That(document.Rows.Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alice"));
        Assert.That(document.Rows[0]["Description"].GetValue<string>(), Is.EqualTo("val|ue"));
        Assert.That(document.Rows[1]["Description"].GetValue<string>(), Is.EqualTo("normal"));
    }

    [Test]
    public void Test_CsvDocument_Quoting_QuotedSharpAtStartOfRow_IsNotComment()
    {
        var csv = "Name,Age\n\"#Alice\",21\nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("#Alice"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    // Comments

    [Test]
    public void Test_CsvDocument_Comments_Basic()
    {
        var csv =
@"#header comment
Name,Age
# row comment
Alex,21
#
# another comment
#
Bob,35
Charles,17
# trailing comment"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("Name"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("Age"));

        Assert.That(document.Rows.Length, Is.EqualTo(3));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));

        Assert.That(document.Rows[2]["Name"].GetValue<string>(), Is.EqualTo("Charles"));
        Assert.That(document.Rows[2]["Age"].GetValue<int>(), Is.EqualTo(17));
    }

    [Test]
    public void Test_CsvDocument_Comments_SharpMustBeComparedByOrdinal()
    {
        var csv =
@"a,b
＃fullwidth,sharp
♯music,sharp"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0][0].GetValue<string>(), Is.EqualTo("＃fullwidth"));
        Assert.That(document.Rows[1][0].GetValue<string>(), Is.EqualTo("♯music"));
    }

    [Test]
    public void Test_CsvDocument_Comments_SharpInMiddleIsNotInterpretedAsComment()
    {
        var csv = "Id,Name\n1,#this is not comment"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("Id"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("Name"));

        Assert.That(document.Rows.Length, Is.EqualTo(1));
        Assert.That(document.Rows[0]["Id"].GetValue<int>(), Is.EqualTo(1));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("#this is not comment"));
    }

    // NewLine

    [Test]
    public void Test_CsvDocument_NewLine_CRLF()
    {
        var csv = "Name,Age\r\nAlex,21\r\nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("Name"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("Age"));

        Assert.That(document.Rows.Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    [Test]
    public void Test_CsvDocument_NewLine_CR()
    {
        var csv = "Name,Age\rAlex,21\rBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("Name"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("Age"));

        Assert.That(document.Rows.Length, Is.EqualTo(2));
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    [Test]
    public void Test_CsvDocument_NewLine_TrailingNewline_SameResult()
    {
        var csvNoTrailing = "Name,Age\nAlex,21\nBob,35"u8;
        var csvWithTrailing = "Name,Age\nAlex,21\nBob,35\n"u8;

        var doc1 = CsvSerializer.ConvertToDocument(csvNoTrailing.ToArray());
        var doc2 = CsvSerializer.ConvertToDocument(csvWithTrailing.ToArray());

        Assert.That(doc1.Rows.Length, Is.EqualTo(doc2.Rows.Length));
        Assert.That(doc1.Rows[0]["Name"].GetValue<string>(), Is.EqualTo(doc2.Rows[0]["Name"].GetValue<string>()));
        Assert.That(doc1.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(doc2.Rows[0]["Age"].GetValue<int>()));
        Assert.That(doc1.Rows[1]["Name"].GetValue<string>(), Is.EqualTo(doc2.Rows[1]["Name"].GetValue<string>()));
        Assert.That(doc1.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(doc2.Rows[1]["Age"].GetValue<int>()));
    }

    // EmptyRows

    [Test]
    public void Test_CsvDocument_EmptyRows_SkipsBlankLines()
    {
        var csv =
            @"Name,Age

Alex,21

Bob,35

Charles,17
"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(3));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));

        Assert.That(document.Rows[2]["Name"].GetValue<string>(), Is.EqualTo("Charles"));
        Assert.That(document.Rows[2]["Age"].GetValue<int>(), Is.EqualTo(17));
    }

    // NoRows

    [Test]
    public void Test_CsvDocument_NoRows_NoHeader_Empty()
    {
        var csv = ""u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            HasHeader = false,
        });

        Assert.That(document.Rows.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_CsvDocument_NoRows_NoHeader_OnlyComments()
    {
        var csv = "#comment1\n#comment2"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            HasHeader = false,
            AllowComments = true,
        });

        Assert.That(document.Rows.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_CsvDocument_NoRows_HeaderOnly()
    {
        var csv = "a,b,c"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(3));
        Assert.That(document.Rows.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_CsvDocument_NoRows_HeaderOnly_LF()
    {
        var csv = "a,b,c\n"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            NewLine = NewLineType.LF,
        });

        Assert.That(document.Header.Length, Is.EqualTo(3));
        Assert.That(document.Rows.Length, Is.EqualTo(0));
    }

    [Test]
    public void Test_CsvDocument_NoRows_OnlyComments()
    {
        var csv = "a,b,c\n#comment1\n#comment2"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray(), new CsvOptions
        {
            AllowComments = true,
        });

        Assert.That(document.Header.Length, Is.EqualTo(3));
        Assert.That(document.Rows.Length, Is.EqualTo(0));
    }

    // MissingFields

    [Test]
    public void Test_CsvDocument_MissingFields_EmptyColumns()
    {
        var csv =
@"a,b
,missing at the first
missing at the last,"u8;
        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0][0].GetValue<sbyte>(), Is.EqualTo(default(sbyte)));
        Assert.That(document.Rows[1][1].GetValue<sbyte>(), Is.EqualTo(default(sbyte)));
        Assert.That(document.Rows[0][0].GetValue<byte>(), Is.EqualTo(default(byte)));
        Assert.That(document.Rows[1][1].GetValue<byte>(), Is.EqualTo(default(byte)));
        Assert.That(document.Rows[0][0].GetValue<short>(), Is.EqualTo(default(short)));
        Assert.That(document.Rows[1][1].GetValue<short>(), Is.EqualTo(default(short)));
        Assert.That(document.Rows[0][0].GetValue<ushort>(), Is.EqualTo(default(ushort)));
        Assert.That(document.Rows[1][1].GetValue<ushort>(), Is.EqualTo(default(ushort)));
        Assert.That(document.Rows[0][0].GetValue<int>(), Is.EqualTo(default(int)));
        Assert.That(document.Rows[1][1].GetValue<int>(), Is.EqualTo(default(int)));
        Assert.That(document.Rows[0][0].GetValue<uint>(), Is.EqualTo(default(uint)));
        Assert.That(document.Rows[1][1].GetValue<uint>(), Is.EqualTo(default(uint)));
        Assert.That(document.Rows[0][0].GetValue<long>(), Is.EqualTo(default(long)));
        Assert.That(document.Rows[1][1].GetValue<long>(), Is.EqualTo(default(long)));
        Assert.That(document.Rows[0][0].GetValue<ulong>(), Is.EqualTo(default(ulong)));
        Assert.That(document.Rows[1][1].GetValue<ulong>(), Is.EqualTo(default(ulong)));
        Assert.That(document.Rows[0][0].GetValue<char>(), Is.EqualTo(default(char)));
        Assert.That(document.Rows[1][1].GetValue<char>(), Is.EqualTo(default(char)));
        Assert.That(document.Rows[0][0].GetValue<string>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<string>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<DayOfWeek>(), Is.EqualTo(default(DayOfWeek))); // enum
        Assert.That(document.Rows[1][1].GetValue<DayOfWeek>(), Is.EqualTo(default(DayOfWeek))); // enum
        Assert.That(document.Rows[0][0].GetValue<DateTime>(), Is.EqualTo(default(DateTime)));
        Assert.That(document.Rows[1][1].GetValue<DateTime>(), Is.EqualTo(default(DateTime)));
        Assert.That(document.Rows[0][0].GetValue<TimeSpan>(), Is.EqualTo(default(TimeSpan)));
        Assert.That(document.Rows[1][1].GetValue<TimeSpan>(), Is.EqualTo(default(TimeSpan)));
        Assert.That(document.Rows[0][0].GetValue<Guid>(), Is.EqualTo(default(Guid)));
        Assert.That(document.Rows[1][1].GetValue<Guid>(), Is.EqualTo(default(Guid)));

        // nullable
        Assert.That(document.Rows[0][0].GetValue<sbyte?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<sbyte?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<byte?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<byte?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<short?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<short?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<ushort?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<ushort?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<int?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<int?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<uint?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<uint?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<long?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<long?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<ulong?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<ulong?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<char?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<char?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<DayOfWeek?>(), Is.Null); // enum
        Assert.That(document.Rows[1][1].GetValue<DayOfWeek?>(), Is.Null); // enum
        Assert.That(document.Rows[0][0].GetValue<DateTime?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<DateTime?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<TimeSpan?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<TimeSpan?>(), Is.Null);
        Assert.That(document.Rows[0][0].GetValue<Guid?>(), Is.Null);
        Assert.That(document.Rows[1][1].GetValue<Guid?>(), Is.Null);
    }

    [Test]
    public void Test_CsvDocument_MissingFields_MissingColumnInShortRow_ByKeyReturnsDefaultElement()
    {
        var csv =
@"Name,Age
Alex
Bob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));
        Assert.That(document.Rows[0].Length, Is.EqualTo(1));

        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<string>(), Is.Null);
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(0));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    [Test]
    public void Test_CsvDocument_MissingFields_MultipleConsecutiveEmptyFields()
    {
        var csv = "h1,h2,h3,h4\na,,,d"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(1));
        Assert.That(document.Rows[0]["h1"].GetValue<string>(), Is.EqualTo("a"));
        Assert.That(document.Rows[0]["h2"].GetValue<string>(), Is.Null);
        Assert.That(document.Rows[0]["h3"].GetValue<string>(), Is.Null);
        Assert.That(document.Rows[0]["h4"].GetValue<string>(), Is.EqualTo("d"));
    }

    [Test]
    public void Test_CsvDocument_MissingFields_WhitespaceInFields()
    {
        var csv = "Name,Age\n  Alex  ,  21  \nBob,35"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        // string preserves surrounding whitespace
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex  "));
        // numeric types trim whitespace
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
    }

    [Test]
    public void Test_CsvDocument_MissingFields_ExtraColumns()
    {
        var csv =
@"Name,Age
Alex,21,extra1,extra2
Bob,35,extra3
Charles,17"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(2));
        Assert.That(document.Rows.Length, Is.EqualTo(3));

        // extra columns are parsed-as-is
        Assert.That(document.Rows[0]["Name"].GetValue<string>(), Is.EqualTo("Alex"));
        Assert.That(document.Rows[0]["Age"].GetValue<int>(), Is.EqualTo(21));
        Assert.That(document.Rows[0].Length, Is.EqualTo(4));

        Assert.That(document.Rows[1]["Name"].GetValue<string>(), Is.EqualTo("Bob"));
        Assert.That(document.Rows[1]["Age"].GetValue<int>(), Is.EqualTo(35));
        Assert.That(document.Rows[1].Length, Is.EqualTo(3));

        Assert.That(document.Rows[2]["Name"].GetValue<string>(), Is.EqualTo("Charles"));
        Assert.That(document.Rows[2]["Age"].GetValue<int>(), Is.EqualTo(17));
        Assert.That(document.Rows[2].Length, Is.EqualTo(2));
    }

    [Test]
    public void Test_CsvDocument_MissingFields_CommasAtEoL()
    {
        var csv = "colA,colB,colC,colD\nA1,B1,C1,\nA2,B2,C2,D2"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Header.Length, Is.EqualTo(4));
        Assert.That(document.Header[0].GetValue<string>(), Is.EqualTo("colA"));
        Assert.That(document.Header[1].GetValue<string>(), Is.EqualTo("colB"));
        Assert.That(document.Header[2].GetValue<string>(), Is.EqualTo("colC"));
        Assert.That(document.Header[3].GetValue<string>(), Is.EqualTo("colD"));

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0].Length, Is.EqualTo(4));
        Assert.That(document.Rows[0][0].GetValue<string>(), Is.EqualTo("A1"));
        Assert.That(document.Rows[0][1].GetValue<string>(), Is.EqualTo("B1"));
        Assert.That(document.Rows[0][2].GetValue<string>(), Is.EqualTo("C1"));
        Assert.That(document.Rows[0][3].GetValue<string>(), Is.Null);

        Assert.That(document.Rows[1].Length, Is.EqualTo(4));
        Assert.That(document.Rows[1][0].GetValue<string>(), Is.EqualTo("A2"));
        Assert.That(document.Rows[1][1].GetValue<string>(), Is.EqualTo("B2"));
        Assert.That(document.Rows[1][2].GetValue<string>(), Is.EqualTo("C2"));
        Assert.That(document.Rows[1][3].GetValue<string>(), Is.EqualTo("D2"));
    }

    [Test]
    public void Test_CsvDocument_MissingFields_CommasAtEoL_Key()
    {
        var csv = "colA,colB,colC,colD\nA1,B1,C1,\nA2,B2,C2,D2"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(document.Rows.Length, Is.EqualTo(2));

        Assert.That(document.Rows[0]["colA"].GetValue<string>(), Is.EqualTo("A1"));
        Assert.That(document.Rows[0]["colB"].GetValue<string>(), Is.EqualTo("B1"));
        Assert.That(document.Rows[0]["colC"].GetValue<string>(), Is.EqualTo("C1"));
        Assert.That(document.Rows[0]["colD"].GetValue<string>(), Is.Null);

        Assert.That(document.Rows[1]["colA"].GetValue<string>(), Is.EqualTo("A2"));
        Assert.That(document.Rows[1]["colB"].GetValue<string>(), Is.EqualTo("B2"));
        Assert.That(document.Rows[1]["colC"].GetValue<string>(), Is.EqualTo("C2"));
        Assert.That(document.Rows[1]["colD"].GetValue<string>(), Is.EqualTo("D2"));
    }

    // Throws

    [Test]
    public void Test_CsvDocument_Throws_MissingKey()
    {
        var csv = "Name,Age\nAlex,21"u8;

        var document = CsvSerializer.ConvertToDocument(csv.ToArray());

        Assert.That(() => document.Rows[0]["Unknown"], Throws.TypeOf<System.Collections.Generic.KeyNotFoundException>());
    }

    [Test]
    public void Test_CsvDocument_Throws_DuplicateKey()
    {
        Assert.That(() => CsvSerializer.ConvertToDocument("a,a"u8.ToArray()), Throws.TypeOf<System.ArgumentException>());
    }
}

// for multiple segments tests
sealed class MemorySegment<T> : ReadOnlySequenceSegment<T>
{
    public MemorySegment(ReadOnlyMemory<T> memory)
    {
        Memory = memory;
    }

    public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
    {
        var segment = new MemorySegment<T>(memory)
        {
            RunningIndex = RunningIndex + Memory.Length,
        };
        Next = segment;
        return segment;
    }
}
