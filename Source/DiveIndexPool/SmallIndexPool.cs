using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DiveIndexPool;

/// <summary>
/// The <see cref="SmallIndexPool{T}"/> class provides a simple, fast, and efficient
/// implementation of an index pool for a small number of indexes. The pool is backed
/// by a bitmask and is suitable for use cases where the number of indexes is less than
/// or equal to 64.
/// </summary>
/// <typeparam name="T">
/// The numeric primitive type of the indexes.
/// The following types are supported:
///   <list type="bullet">
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.byte">byte</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.char">char</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.int32">int</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.int64">long</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.intptr">nint</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uintptr">nuint</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.sbyte">sbyte</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.int16">short</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uint32">uint</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uint64">ulong</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uint16">ushort</see>
///     </item>
///   </list>
/// </typeparam>
/// <exception cref="BackingTypeUnsupportedException">
/// Thrown if the backing type <typeparamref name="T"/> is not a supported numeric primitive
/// The following types are supported:
///   <list type="bullet">
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.byte">byte</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.char">char</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.int32">int</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.int64">long</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.intptr">nint</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uintptr">nuint</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.sbyte">sbyte</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.int16">short</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uint32">uint</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uint64">ulong</see>
///     </item>
///     <item>
///     <see href="https://docs.microsoft.com/dotnet/api/system.uint16">ushort</see>
///     </item>
///   </list>
/// </exception>
/// <threadsafety instance="false"/>
/// <seealso cref="LargeIndexPool{T}"/>
internal sealed class SmallIndexPool<T>
    : IndexPool<T>
    where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
{
    /// <summary>
    /// The bitmask used to track free indexes.
    /// </summary>
    private ulong _Bitmask;

    /// <inheritdoc/>
    public override bool IsThreadSafe => false;

    /// <inheritdoc/>
    public override ulong Capacity { get; }

    /// <inheritdoc/>
    public override ulong Count => _Bitmask == 0UL ? 0UL : (ulong)BitOperations.PopCount(_Bitmask);

    /// <inheritdoc/>
    public override T StartIndex { get; }

    /// <inheritdoc/>
    public override bool IsEmpty => _Bitmask == 0UL;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmallIndexPool{T}"/> class.
    /// </summary>
    /// <param name="capacity">
    /// Optional. The number of indexes in the pool.
    /// The value is clamped to a minimum of 1 and a maximum of 64.
    /// Defaults to <see cref="DefaultSmallCapacity"/>.
    /// </param>
    /// <param name="startIndex">
    /// Optional. The starting index value. The default value is 0.
    /// </param>
    [MethodImpl(AggressiveInlining)]
    internal SmallIndexPool(byte capacity = 64, [MaybeNull] T? startIndex = null)
    {
        StartIndex = startIndex ?? T.Zero;

        Capacity = capacity;
        _Bitmask = (capacity == 64) ? ulong.MaxValue : ((1UL << capacity) - 1);
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override bool IsValid(T index)
        => index >= T.Zero && T.CreateChecked(Capacity - 1) >= index - StartIndex;

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override void Reset()
        => _Bitmask = (Capacity == 64) ? ulong.MaxValue : ((1UL << (byte)Capacity) - 1);

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override void Clear() => _Bitmask = 0UL;

    /// <inheritdoc/>
    public override T Take()
    {
        if (_Bitmask == 0UL)
            return Values<T>.InvalidIndex;

        var bitPosition = (byte)BitOperations.TrailingZeroCount(_Bitmask);

        _Bitmask &= ~(1UL << bitPosition);

        return StartIndex + T.CreateChecked(bitPosition);
    }

    /// <inheritdoc/>
    public override T[] Take(ulong count)
    {
        if (count > Capacity)
            count = Capacity;

        var countInt = Convert.ToInt32(count);
        var result = new T[countInt];

        Array.Fill(result, Values<T>.InvalidIndex);

        for (var i = 0; i < countInt; i++)
            result[i] = Take();

        return result;
    }

    /// <inheritdoc/>
    public override bool TryTake(out T index)
    {
        if (_Bitmask == 0UL)
        {
            index = Values<T>.InvalidIndex;
            return false;
        }

        var bitPosition = BitOperations.TrailingZeroCount(_Bitmask);

        _Bitmask &= ~(1UL << bitPosition);
        index = StartIndex + T.CreateChecked(bitPosition);

        return true;
    }

    /// <inheritdoc/>
    public override T[] TakeAll()
    {
        var bitmask = _Bitmask;
        var freeCount = BitOperations.PopCount(bitmask);
        var result = new T[freeCount];

        var index = 0;

        while (bitmask != 0UL)
        {
            var bitPosition = BitOperations.TrailingZeroCount(bitmask);
            result[index++] = StartIndex + T.CreateChecked(bitPosition);
            bitmask &= ~(1UL << bitPosition);
        }

        _Bitmask = 0UL;

        return result;
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override bool TryTakeAll(out T[] indexes)
    {
        indexes = TakeAll();
        return indexes.Length > 0;
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override T Peek()
    {
        if (_Bitmask == 0UL)
            return Values<T>.InvalidIndex;

        var bitPosition = BitOperations.TrailingZeroCount(_Bitmask);
        return StartIndex + T.CreateChecked(bitPosition);
    }

    /// <inheritdoc/>
    public override bool TryPeek(out T index)
    {
        if (_Bitmask == 0UL)
        {
            index = Values<T>.InvalidIndex;
            return false;
        }

        var bitPosition = BitOperations.TrailingZeroCount(_Bitmask);
        index = StartIndex + T.CreateChecked(bitPosition);

        return true;
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override bool Contains(T index)
    {
        if (!IsValid(index))
            return false;

        var shift = Convert.ToInt32(index - StartIndex);

        return ((_Bitmask >> shift) & 1UL) == 1UL;
    }

    /// <inheritdoc/>
    public override bool Return(T index)
    {
        if (!IsValid(index))
            return false;

        var shift = Convert.ToInt32(index - StartIndex);

        if (shift < 0 || shift >= (int)Capacity)
            return false;

        _Bitmask |= 1UL << shift;

        return true;
    }

    /// <inheritdoc/>
    public override ulong ReturnAll(params scoped ReadOnlySpan<T> indexes)
    {
        ulong returned = 0;

        foreach (T index in indexes)
        {
            if (Return(index))
                returned++;
        }

        return returned;
    }

    /// <inheritdoc/>
    public override T[] ToArray()
    {
        var results = new T[(int)Count];
        var index = 0;

        for (byte i = 0; i < Capacity; i++)
        {
            if (((_Bitmask >> i) & 1UL) == 1UL)
                results[index++] = StartIndex + T.CreateChecked(i);
        }

        return results;
    }

    /// <inheritdoc/>
    /// <seealso cref="GetEnumerator"/>
    [MethodImpl(AggressiveInlining)]
    public override IEnumerator<T> EnumerateAllocated()
    {
        for (byte i = 0; i < Capacity; i++)
        {
            if (((_Bitmask >> i) & 1UL) == 0UL)
                yield return StartIndex + T.CreateChecked(i);
        }
    }

    /// <inheritdoc/>
    /// <seealso cref="EnumerateAllocated"/>
    [MethodImpl(AggressiveInlining)]
    public override IEnumerator<T> GetEnumerator()
    {
        for (byte i = 0; i < Capacity; i++)
        {
            if (((_Bitmask >> i) & 1UL) == 1UL)
                yield return StartIndex + T.CreateChecked(i);
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode() => _Bitmask.GetHashCode() ^ StartIndex.GetHashCode();
}
