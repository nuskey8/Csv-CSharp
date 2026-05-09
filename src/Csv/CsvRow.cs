using System.Runtime.CompilerServices;

namespace Csv;

public readonly record struct CsvHeader
{
    internal CsvHeader(CsvElement[] elements)
    {
        this.elements = elements;
    }

    readonly CsvElement[] elements;

    public int Length => elements.Length;

    public CsvElement this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return elements[index];
        }
    }
}


public readonly record struct CsvRow
{
    internal CsvRow(CsvDocument document, CsvElement[] elements)
    {
        this.document = document;
        this.elements = elements;
    }

    readonly CsvDocument document;
    readonly CsvElement[] elements;

    public int Length => elements.Length;

    public CsvElement this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return elements[index];
        }
    }
    
    public CsvElement this[string key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var index = document.columnCache[key];
            if ((uint)index >= (uint)elements.Length) return default;
            return elements[index];
        }
    }
}
