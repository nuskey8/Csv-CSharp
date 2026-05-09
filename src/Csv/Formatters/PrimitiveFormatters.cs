namespace Csv.Formatters;

public sealed class BoolFormatter : ICsvFormatter<bool>
{
    public static readonly BoolFormatter Instance = new();

    public bool Deserialize(ref CsvReader reader)
    {
        return reader.ReadBoolean();
    }

    public void Serialize(ref CsvWriter writer, bool value)
    {
        writer.WriteBoolean(value);
    }
}

public sealed class ByteFormatter : ICsvFormatter<byte>
{
    public static readonly ByteFormatter Instance = new();

    public byte Deserialize(ref CsvReader reader)
    {
        return reader.ReadByte();
    }

    public void Serialize(ref CsvWriter writer, byte value)
    {
        writer.WriteByte(value);
    }
}

public sealed class SByteFormatter : ICsvFormatter<sbyte>
{
    public static readonly SByteFormatter Instance = new();

    public sbyte Deserialize(ref CsvReader reader)
    {
        return reader.ReadSByte();
    }

    public void Serialize(ref CsvWriter writer, sbyte value)
    {
        writer.WriteSByte(value);
    }
}

public sealed class Int16Formatter : ICsvFormatter<short>
{
    public static readonly Int16Formatter Instance = new();

    public short Deserialize(ref CsvReader reader)
    {
        return reader.ReadInt16();
    }

    public void Serialize(ref CsvWriter writer, short value)
    {
        writer.WriteInt16(value);
    }
}

public sealed class UInt16Formatter : ICsvFormatter<ushort>
{
    public static readonly UInt16Formatter Instance = new();

    public ushort Deserialize(ref CsvReader reader)
    {
        return reader.ReadUInt16();
    }

    public void Serialize(ref CsvWriter writer, ushort value)
    {
        writer.WriteUInt16(value);
    }
}

public sealed class Int32Formatter : ICsvFormatter<int>
{
    public static readonly Int32Formatter Instance = new();

    public int Deserialize(ref CsvReader reader)
    {
        return reader.ReadInt32();
    }

    public void Serialize(ref CsvWriter writer, int value)
    {
        writer.WriteInt32(value);
    }
}

public sealed class UInt32Formatter : ICsvFormatter<uint>
{
    public static readonly UInt32Formatter Instance = new();

    public uint Deserialize(ref CsvReader reader)
    {
        return reader.ReadUInt32();
    }

    public void Serialize(ref CsvWriter writer, uint value)
    {
        writer.WriteUInt32(value);
    }
}

public sealed class Int64Formatter : ICsvFormatter<long>
{
    public static readonly Int64Formatter Instance = new();

    public long Deserialize(ref CsvReader reader)
    {
        return reader.ReadInt64();
    }

    public void Serialize(ref CsvWriter writer, long value)
    {
        writer.WriteInt64(value);
    }
}

public sealed class UInt64Formatter : ICsvFormatter<ulong>
{
    public static readonly UInt64Formatter Instance = new();

    public ulong Deserialize(ref CsvReader reader)
    {
        return reader.ReadUInt64();
    }

    public void Serialize(ref CsvWriter writer, ulong value)
    {
        writer.WriteUInt64(value);
    }
}

public sealed class CharFormatter : ICsvFormatter<char>
{
    public static readonly CharFormatter Instance = new();

    public char Deserialize(ref CsvReader reader)
    {
        return reader.ReadChar();
    }

    public void Serialize(ref CsvWriter writer, char value)
    {
        writer.WriteChar(value);
    }
}

public sealed class StringFormatter : ICsvFormatter<string?>
{
    public static readonly StringFormatter Instance = new();

    public string? Deserialize(ref CsvReader reader)
    {
        return reader.ReadString();
    }

    public void Serialize(ref CsvWriter writer, string? value)
    {
        writer.WriteString(value);
    }
}
