using System.Buffers;
using System.Text;
using Csv.Internal;

namespace Csv;

[Serializable]
public sealed class CsvDocument
{
    internal CsvDocument(ReadOnlySequence<byte> sequence, CsvOptions options)
    {
        var buffer = new TempList<byte>();
        var elements = new TempList<CsvElement>();
        var rows = new TempList<CsvRow>();

        try
        {
            this.sequence = sequence;
            this.options = options;

            var reader = new CsvReader(sequence, options);
            var allowComments = options.AllowComments;

            if (options.HasHeader)
            {
                while (reader.Remaining > 0)
                {
                    if (reader.TryReadEndOfLine()) continue;
                    if (allowComments && reader.TrySkipComment(false)) continue;

                    break;
                }

                var columnIndex = 0;
                while (reader.Remaining > 0)
                {
                    var index = reader.Consumed;
                    var length = reader.ReadUtf8(ref buffer);
                    elements.Add(new(this, index, length));

                    columnCache.Add(Encoding.UTF8.GetString(buffer.AsSpan()), columnIndex);
                    buffer.Clear();

                    columnIndex++;

                    if (reader.TryReadEndOfLine()) break;
                    reader.TryReadSeparator(false);
                }

                header = new(elements.AsSpan().ToArray());
                elements.Clear();
            }

            while (reader.Remaining > 0)
            {
                if (reader.TryReadEndOfLine()) continue;
                if (allowComments && reader.TrySkipComment(false)) continue;

                var index = reader.Consumed;
                var length = reader.SkipField();
                elements.Add(new(this, index, length));

                reader.TryReadSeparator(false);

                if (reader.Remaining == 0 || reader.TryReadEndOfLine())
                {
                    rows.Add(new(this, elements.AsSpan().ToArray()));
                    elements.Clear();
                    continue;
                }
            }

            this.rows = rows.AsSpan().ToArray();
        }
        finally
        {
            buffer.Dispose();
            elements.Dispose();
            rows.Dispose();
        }
    }

    readonly ReadOnlySequence<byte> sequence;
    internal readonly Dictionary<string, int> columnCache = [];
    internal readonly CsvOptions options;

    readonly CsvHeader header;
    readonly CsvRow[] rows;

    internal ReadOnlySequence<byte> Sequence => sequence;
    public CsvHeader Header => header;
    public ReadOnlySpan<CsvRow> Rows => rows;
}