// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Csv.Internal;

internal static class SequenceReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryRead(ref this SequenceReader<byte> reader, out int value)
    {
        var currentSpan = reader.CurrentSpan;
        var currentSpanIndex = reader.CurrentSpanIndex;
        if (currentSpan.Length - currentSpanIndex < sizeof(int))
            return TryReadMultisegment(ref reader, out value);

        value = Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref MemoryMarshal.GetReference(currentSpan), currentSpanIndex));
        reader.Advance(sizeof(int));
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryReadMultisegment(ref SequenceReader<byte> reader, out int value)
    {
        // Not enough data in the current segment, try to peek for the data we need.
        int buffer = default;
        Span<byte> tempSpan = MemoryMarshal.CreateSpan(ref Unsafe.As<int, byte>(ref buffer), 4);

        if (!reader.TryCopyTo(tempSpan))
        {
            value = default;
            return false;
        }

        value = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(tempSpan));
        reader.Advance(sizeof(int));
        return true;
    }
}