
using System.Buffers;
using System.Text;
using Csv.Annotations;

namespace Csv.Tests;

public class SerializeTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {

    }

    [Test]
    public void Test_GetSerializer()
    {
        Assert.That(CsvSerializer.GetSerializer<User>(), Is.Not.Null);
    }

    [Test]
    public void Test_Serialize_Standard()
    {
        User[] users = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        var bytes = CsvSerializer.Serialize(users);
        var str = Encoding.UTF8.GetString(bytes);

        Assert.That(str, Is.EqualTo(
@"Name,Age
Alex,21
Bob,35
Charles,17"
        ));
    }

    [Test]
    public void Test_Serialize_WithQuote()
    {
        User[] users = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        var bytes = CsvSerializer.Serialize(users, CsvSerializer.DefaultOptions with
        {
            QuoteMode = QuoteMode.All,
        });
        var str = Encoding.UTF8.GetString(bytes);

        Assert.That(str, Is.EqualTo(
@"""Name"",""Age""
""Alex"",""21""
""Bob"",""35""
""Charles"",""17"""
        ));
    }

    [Test]
    public void Test_Deserialize_Simple()
    {
        var csv =
@"Name,Age
Alex,21  
Bob,35
Charles,17"u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_Simple_WithBuffer()
    {
        var csv =
@"Name,Age
Alex,21  
Bob,35
Charles,17"u8;

        User[] actual = new User[3];
        CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()), actual);

        User[] expected = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_Empty()
    {
        var csv =
@"Name,Age
,21  
Bob,
Charles,17"u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = null, Age = 21 },
            new() { Name = "Bob", Age = 0 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_EmptyRow()
    {
        var csv =
@"Name,Age


Alex,21

Bob,35

Charles,17"u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_FieldCountMismatch()
    {
        var csv =
@"Name,Age
Alex,
Bob,35
Charles,"u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = "Alex", Age = 0 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 0 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_Complex()
    {
        var csv =
@"# This is comment!
Name,Age
Alex,""21""    
""Bob"",35
Charles,17"u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_ExtraColumns()
    {
        var csv =
@"Name,Age,Dummy1,Dummy2
Alex,21,1,""a""
Bob,35,25,""b""
Charles,17,23,""c"""u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_Deserialize_Comments()
    {
        var csv =
@"# hello!
# how are you?
Name,Age
# comment1
Alex,21  
# comment2
Bob,35
# comment3
# comment4
Charles,17
# comment5"u8;

        User[] actual = CsvSerializer.Deserialize<User>(new ReadOnlySequence<byte>(csv.ToArray()));
        User[] expected = [
            new() { Name = "Alex", Age = 21 },
            new() { Name = "Bob", Age = 35 },
            new() { Name = "Charles", Age = 17 }
        ];

        CollectionAssert.AreEqual(expected, actual);
    }
}

[CsvObject]
public partial record User
{
    [Column(0)]
    public string? Name { get; set; }
    [Column(1)]
    public int Age { get; set; }
}