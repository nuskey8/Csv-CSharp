using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace Csv;

public readonly struct CsvElement
{
    public CsvElement(CsvDocument document, long index, long length)
    {
        this.document = document;
        this.sequence = document.Sequence.Slice(index, length);
    }

    readonly CsvDocument document;
    readonly ReadOnlySequence<byte> sequence;

    public ReadOnlySequence<byte> Sequence => sequence;

    public T? GetValue<T>()
    {
        if (document is null) return default;
        var reader = new CsvReader(sequence, document.options);
        return document.options.FormatterProvider.GetFormatterWithVarify<T>().Deserialize(ref reader);
    }
}