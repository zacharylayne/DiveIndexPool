using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DiveIndexPool;

/// <summary>
/// The <see cref="LargeIndexPool{T}"/> class provides a simple, fast, and efficient
/// implementation of an index pool for a large number of indexes. The pool is backed
/// by an array of bitmasks and is suitable for use cases where the number of indexes
/// is greater than 64.
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
/// <seealso cref="SmallIndexPool{T}"/>
/// <threadsafety static="true" instance="false"/>
internal sealed class LargeIndexPool<T>
: IndexPool<T>
    where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
{
    /// <summary>
    /// An array of 64-bit bitmasks to track the allocation status of indexes.
    /// </summary>
    private readonly ulong[] _Bitmasks;

    /// <summary>
    /// The number of 64-bit bitmasks needed to cover <see cref="Capacity"/> indexes.
    /// </summary>
    private readonly int _BitmaskCount;

    /// <summary>
    /// The bitmask for the remainder of indexes when the capacity is not a multiple of 64.
    /// </summary>
    private readonly ulong _RemainderBitmask;

    /// <inheritdoc/>
    public override bool IsThreadSafe => false;

    /// <inheritdoc/>
    public override ulong Capacity { get; }

    /// <inheritdoc/>
    public override ulong Count
    {
        [MethodImpl(AggressiveInlining)]
        get
        {
            ulong count = 0;

            for (var i = 0; i < _BitmaskCount; i++)
                count += (ulong)BitOperations.PopCount(_Bitmasks[i]);

            return count;
        }
    }

    /// <summary>
    /// The backing field for <see cref="StartIndex"/>.
    /// </summary>
    private readonly ulong _StartIndex;

    /// <inheritdoc/>
    public override T StartIndex => T.CreateChecked(_StartIndex);

    /// <inheritdoc/>
    public override bool IsEmpty => Count == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="LargeIndexPool{T}"/> class.
    /// </summary>
    /// <param name="capacity">
    /// The total number of indexes in the pool. The value is clamped between 1
    /// and <see cref="AbsoluteMaxCapacity"/>.
    /// </param>
    /// <param name="startIndex">
    /// The starting index value. Defaults to zero if not provided.
    /// </param>
    internal LargeIndexPool(ulong capacity, [MaybeNull] T? startIndex = null)
    {
        if (capacity < 1)
            capacity = 1;

        if (capacity > AbsoluteMaxCapacity)
            capacity = AbsoluteMaxCapacity;

        Capacity = capacity;

        _StartIndex = startIndex is null ? 0
                      : Convert.ToUInt64(startIndex);

        _BitmaskCount = (int)((capacity + 63UL) / 64UL);

        _Bitmasks = new ulong[_BitmaskCount];

        for (var i = 0; i < _BitmaskCount - 1; i++)
            _Bitmasks[i] = ulong.MaxValue;

        var remainder = (int)(capacity % 64);
        _RemainderBitmask = remainder == 0 ? ulong.MaxValue
                       : ((1UL << remainder) - 1);

        _Bitmasks[_BitmaskCount - 1] = _RemainderBitmask;
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override bool IsValid(T index)
        => index >= T.Zero && Capacity >= Convert.ToUInt64(index) - _StartIndex;

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override void Reset()
    {
        for (var i = 0; i < _BitmaskCount - 1; i++)
            _Bitmasks[i] = ulong.MaxValue;

        _Bitmasks[_BitmaskCount - 1] = _RemainderBitmask;
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override void Clear()
    {
        for (var i = 0; i < _BitmaskCount; i++)
            _Bitmasks[i] = 0UL;
    }

    /// <inheritdoc/>
    public override T Take()
    {
        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                var bitPosition = BitOperations.TrailingZeroCount(bitmask);

                _Bitmasks[i] &= ~(1UL << bitPosition);
                var overallIndex = (i * 64) +  bitPosition;

                return T.CreateChecked(_StartIndex + (ulong)overallIndex);
            }
        }

        return Values<T>.InvalidIndex;
    }

    /// <inheritdoc/>
    public override bool TryTake(out T index)
    {
        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                var bitPosition = BitOperations.TrailingZeroCount(bitmask);

                _Bitmasks[i] &= ~(1UL << bitPosition);
                var overallIndex = (i * 64) +  bitPosition;

                index = T.CreateChecked(_StartIndex + (ulong)overallIndex);

                return true;
            }
        }

        index = Values<T>.InvalidIndex;
        return false;
    }

    /// <inheritdoc/>
    public override T[] Take(ulong count)
    {
        var countInt = count > Capacity ? Capacity
                       : count;

        var result = new T[countInt];

        for (ulong i = 0; i < countInt; i++)
            result[i] = Take();

        return result;
    }

    /// <inheritdoc/>
    public override T[] TakeAll()
    {
        List<T> list = [];

        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                for (var bit = 0; bit < 64; bit++)
                {
                    if (i == _BitmaskCount - 1 && (ulong)bit >= Capacity % 64 && Capacity % 64 != 0)
                        break;

                    if (((bitmask >> bit) & 1UL) == 1UL)
                    {
                        var overallIndex = (i * 64) + bit;
                        list.Add(T.CreateChecked(_StartIndex + (ulong)overallIndex));
                    }
                }
            }

            _Bitmasks[i] = 0UL;
        }

        return [.. list];
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
    public override bool TryTakeAll(out T[] indexes)
    {
        indexes = TakeAll();
        return indexes.Length > 0;
    }

    /// <inheritdoc/>
    public override T Peek()
    {
        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                var bitPosition = BitOperations.TrailingZeroCount(bitmask);
                var overallIndex = (i * 64) + bitPosition;

                return T.CreateChecked(_StartIndex +  (ulong)overallIndex);
            }
        }

        return Values<T>.InvalidIndex;
    }

    /// <inheritdoc/>
    public override bool TryPeek(out T index)
    {
        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                var bitPosition = BitOperations.TrailingZeroCount(bitmask);
                var overallIndex = (i * 64) + bitPosition;
                index = T.CreateChecked(_StartIndex +  (ulong)overallIndex);

                return true;
            }
        }

        index = Values<T>.InvalidIndex;
        return false;
    }

    /// <inheritdoc/>
    public override bool Contains(T index)
    {
        if (!IsValid(index))
            return false;

        var offset = (ulong)Convert.ToInt32(index) - _StartIndex;
        var bitmask = offset / 64;
        var bit = offset % 64;

        return ((_Bitmasks[bitmask] >> (int)bit) & 1UL) == 1UL;
    }

    /// <inheritdoc/>
    public override bool Return(T index)
    {
        var offset = (ulong)Convert.ToInt32(index) - _StartIndex;

        if (offset >= Capacity)
            return false;

        var bitmask = offset / 64;
        var bit = offset % 64;

        _Bitmasks[bitmask] |= 1UL << (int)bit;
        return true;
    }

    /// <inheritdoc/>
    [MethodImpl(AggressiveInlining)]
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
        List<T> list = [];

        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                for (var bit = 0; bit < 64; bit++)
                {
                    if (i == _BitmaskCount - 1 && (ulong)bit >= Capacity % 64 && Capacity % 64 != 0)
                        break;

                    if (((bitmask >> bit) & 1UL) == 1UL)
                    {
                        var overallIndex = (i * 64) + bit;
                        list.Add(T.CreateChecked(_StartIndex + (ulong)overallIndex));
                    }
                }
            }
        }

        return [.. list];
    }

    /// <inheritdoc/>
    public override IEnumerator<T> EnumerateAllocated()
    {
        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != ulong.MaxValue)
            {
                for (var bit = 0; bit < 64; bit++)
                {
                    if (i == _BitmaskCount - 1 && (ulong)bit >= Capacity % 64 && Capacity % 64 != 0)
                        break;

                    if (((bitmask >> bit) & 1UL) == 0UL)
                    {
                        var overallIndex = (i * 64) + bit;
                        yield return T.CreateChecked(_StartIndex + (ulong)overallIndex);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public override IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < _BitmaskCount; i++)
        {
            var bitmask = _Bitmasks[i];

            if (bitmask != 0UL)
            {
                for (var bit = 0; bit < 64; bit++)
                {
                    if (i == _BitmaskCount - 1 && (ulong)bit >= Capacity % 64 && Capacity % 64 != 0)
                        break;

                    if (((bitmask >> bit) & 1UL) == 1UL)
                    {
                        var overallIndex = (i * 64) + bit;
                        yield return T.CreateChecked(_StartIndex + (ulong)overallIndex);
                    }
                }
            }
        }
    }
}
