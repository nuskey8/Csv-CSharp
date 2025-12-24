namespace Csv.Internal;

internal static class CsvConstants
{
    public const int TRUEBytesLittleEndian = ((byte)'T') | ((byte)'R' << 8) | ((byte)'U' << 16) | ((byte)'E' << 24);
    public const int TrueBytesLittleEndian = ((byte)'T') | ((byte)'r' << 8) | ((byte)'u' << 16) | ((byte)'e' << 24);
    public const int trueBytesLittleEndian = ((byte)'t') | ((byte)'r' << 8) | ((byte)'u' << 16) | ((byte)'e' << 24);
    public const int FALSEBytesLittleEndian = ((byte)'F') | ((byte)'A' << 8) | ((byte)'L' << 16) | ((byte)'S' << 24);
    public const int FalseBytesLittleEndian = ((byte)'F') | ((byte)'a' << 8) | ((byte)'l' << 16) | ((byte)'s' << 24);
    public const int falseBytesLittleEndian = ((byte)'f') | ((byte)'a' << 8) | ((byte)'l' << 16) | ((byte)'s' << 24);

    public const int TRUEBytesBigEndian = ((byte)'T' << 24) | ((byte)'R' << 16) | ((byte)'U' << 8) | ((byte)'E');
    public const int TrueBytesBigEndian = ((byte)'T' << 24) | ((byte)'r' << 16) | ((byte)'u' << 8) | ((byte)'e');
    public const int trueBytesBigEndian = ((byte)'t' << 24) | ((byte)'r' << 16) | ((byte)'u' << 8) | ((byte)'e');
    public const int FALSEBytesBigEndian = ((byte)'F' << 24) | ((byte)'A' << 16) | ((byte)'L' << 8) | ((byte)'S');
    public const int FalseBytesBigEndian = ((byte)'F' << 24) | ((byte)'a' << 16) | ((byte)'l' << 8) | ((byte)'s');
    public const int falseBytesBigEndian = ((byte)'f' << 24) | ((byte)'a' << 16) | ((byte)'l' << 8) | ((byte)'s');
}