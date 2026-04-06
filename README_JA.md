# Csv-CSharp

[![NuGet](https://img.shields.io/nuget/v/CsvCSharp.svg)](https://www.nuget.org/packages/CsvCSharp)
[![Releases](https://img.shields.io/github/release/nuskey8/Csv-CSharp.svg)](https://github.com/nuskey8/Csv-CSharp/releases)
[![GitHub license](https://img.shields.io/github/license/nuskey8/Csv-CSharp.svg)](./LICENSE)

[English]((./README.md)) | 日本語

![img](docs/img1.png)

Csv-CSharpは.NET、Unity向けの非常に高速なCSV(TSV)パーサです。UTF-8バイト配列を直接解析する設計とSource Generatorの活用により、ゼロ(または非常に少ない)アロケーションでCSV(TSV)とオブジェクト配列間のシリアライズおよびデシリアライズを可能にします。

## インストール

### NuGet packages

Csv-CSharpを利用するには.NET Standard2.1以上が必要です。パッケージはNuGetから入手できます。

### .NET CLI

```ps1
dotnet add package CsvCSharp
```

### Package Manager

```ps1
Install-Package CsvCSharp
```

### Unity

[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)を利用することで、Csv-CSharpをUnityにインストールできます。詳細はNuGetForUnityのREADMEを参照してください。

## クイックスタート

Csv-CSharpはCSVデータをclassまたはstructの配列としてシリアライズおよびデシリアライズします。

classまたはstructを定義し、`[CsvObject]`属性と`partial`キーワードを付加します。

```cs
[CsvObject]
public partial class Person
{
    [Column(0)]
    public string Name { get; set; }

    [Column(1)]
    public int Age { get; set; }
}
```

`[CsvObject]`属性でマークした型のpublicなフィールドおよびプロパティには全て`[Column]`または`[IgnoreMember]`属性を付加する必要があります。(どちらの属性も見つからないメンバーにはAnalyzerがコンパイルエラーを報告します。)

`[Column]`にはint型で列のインデックスを指定するか、string型でヘッダ名を指定することができます。

この型をcsvにシリアライズ、またはcsvからデシリアライズするには`CsvSerializer`を使用します。

```cs
var array = new Person[]
{
    new() { Name = "Alice", Age = 18 },
    new() { Name = "Bob", Age = 23 },
    new() { Name = "Carol", Age = 31 },
};

// Person[] -> CSV (UTF-8)
byte[] csv = CsvSerializer.Serialize(array);

// Person[] -> CSV (UTF-16)
string csvText = CsvSerializer.SerializeToString(array);

// CSV (UTF-8) -> Person[]
array = CsvSerializer.Deserialize<Person>(csv);

// CSV (UTF-16) -> Person[]
array = CsvSerializer.Deserialize<Person>(csvText);
```

`Serialize`はUTF-8でエンコードされた`byte[]`を返すオーバーロードのほか、`Stream`や`IBufferWriter<byte>`を渡して書き込みを行うことも可能です。`Deserialize`はUTF-8バイト配列の`byte[]`を受け取るほか、`string`、`Stream`、`ReadOnlySequence<byte>`にも対応しています。

フィールドおよびプロパティに使用できる型は、デフォルトでは`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `char`, `string`, `Enum`, `Nullable<T>`, `DateTime`, `TimeSpan`, `Guid`に対応しています。これ以外の型に対応したい場合は機能拡張のセクションを参照してください。

## シリアライズ

`CsvSerializer`に渡すclassまたはstructには`[CsvObject]`属性と`partial`キーワードを付加します。

デフォルトでは、`[Column]`属性が付加されたメンバーのみがシリアライズおよびデシリアライズの対象になります。publicメンバーには`[Column]`または`[IgnoreMember]`が必須で、どちらもない場合はAnalyzerがコンパイルエラーを報告します。privateメンバーはデフォルトでは無視されますが、`[Column]`属性を付加することで対象に含めることができます。

```cs
[CsvObject]
public partial class Person
{
    [Column(0)]
    public string Name { get; set; } // シリアライズ対象 (public、[Column]が必須)

    [Column(1)]
    int age; // シリアライズ対象 (private、[Column]で明示的に指定)

    [IgnoreMember]
    public int Age => age; // シリアライズ対象外 ([IgnoreMember]で除外)
}
```

インデックスではなくヘッダ名を指定したい場合は、ヘッダ名を文字列で渡します。

```cs
[CsvObject]
public partial class Person
{
    [Column("name")]
    public string Name { get; set; }

    [Column("age")]
    public int Age { get; set; }
}
```

メンバー名をそのままカラム名として使用する場合は`[CsvObject(keyAsPropertyName: true)]`を指定します。この場合、`[Column]`属性は必要ありません。

```cs
[CsvObject(keyAsPropertyName: true)]
public partial class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

## CsvDocument

CSVのフィールドを直接解析したい場合には`CsvDocument`を使用することができます。

```cs
var array = new Person[]
{
    new() { Name = "Alice", Age = 18 },
    new() { Name = "Bob", Age = 23 },
    new() { Name = "Carol", Age = 31 },
};

byte[] csv = CsvSerializer.Serialize(array);

// CSV (UTF-8) -> CsvDocument
var document = CsvSerializer.ConvertToDocument(csv);

foreach (var row in document.Rows)
{
    var name = row["Name"].GetValue<string>();
    var age = row["Age"].GetValue<int>();
}
```

## オプション

Serialize/Deserializeに`CsvOptions`を渡すことでcsvの設定を変更することができます。

```cs
CsvSerializer.Serialize(array, new CsvOptions()
{
    HasHeader = true, // ヘッダ行を含むか
    AllowComments = true, // '#'から始まるコメントを許可するか
    NewLine = NewLineType.LF, // 改行コード
    Separator = SeparatorType.Comma, // 区切り文字
    QuoteMode = QuoteMode.Minimal, // フィールドをダブルクォーテーションで囲む条件 (Minimalはエスケープが必要な文字を含むフィールドのみ囲む)
    FormatterProvider = StandardFormatterProvider.Instance, // 使用するICsvFormatterProvider
});
```

## CSVの仕様

Csv-CSharpのデフォルトの設定は概ね[RFC 4180](https://www.rfc-editor.org/rfc/rfc4180.html)で規定された仕様に従いますが、パフォーマンスや実用性の観点から、いくつかの意図的な逸脱があります。

- 改行コードのデフォルトはCRLFではなくLFです。
- フィールド数が不一致のレコードもエラーなく読み取りが可能です。不足するフィールドはデフォルト値のままになります。

## 機能拡張

フィールドのシリアライズおよびデシリアライズをカスタマイズするためのインターフェースとして`ICsvFormatter<T>`と`ICsvFormatterProvider`が提供されています。

カスタム型のシリアライズおよびデシリアライズには`ICsvFormatter<T>`を使用します。例として`int`型をラップする構造体に対応したFormatterの実装を示します。

```cs
public struct Foo
{
    public int Value;

    public Foo(int value)
    {
        this.Value = value;
    }
}

public sealed class FooFormatter : ICsvFormatter<Foo>
{
    public Foo Deserialize(ref CsvReader reader)
    {
        var value = reader.ReadInt32();
        return new Foo(value);
    }

    public void Serialize(ref CsvWriter writer, Foo value)
    {
        writer.WriteInt32(value.Value);
    }
}
```

続いてFormatterを取得するためのFormatterProviderを実装します。

```cs
public class CustomFormatterProvider : ICsvFormatterProvider
{
    public static readonly ICsvFormatterProvider Instance = new CustomFormatterProvider();

    CustomFormatterProvider()
    {
    }

    static CustomFormatterProvider()
    {
        FormatterCache<Foo>.Formatter = new FooeFormatter();
    }

    public ICsvFormatter<T>? GetFormatter<T>()
    {
        return FormatterCache<T>.Formatter;
    }

    static class FormatterCache<T>
    {
        public static readonly ICsvFormatter<T> Formatter;
    }
}
```

上記の`CustomFormatterProvider`は`Foo`構造体にのみ対応しています。デフォルトの対応型も扱うには、`CompositeFormatterProvider`を使って`StandardFormatterProvider`と組み合わせ、`CsvOptions`に渡します。

```cs
// CompositeFormatterProviderで複数のFormatterProviderをまとめる
var provider = CompositeFormatterProvider.Create(
    CustomFormatterProvider.Instance,
    StandardFormatterProvider.Instance
);

CsvSerializer.Serialize(array, new CsvOptions()
{
    FormatterProvider = provider
});
```

## ライセンス

このライブラリはMITライセンスの下に公開されています。