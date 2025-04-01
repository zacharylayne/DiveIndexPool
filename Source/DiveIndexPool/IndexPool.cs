using System.Numerics;

#pragma warning disable IDE0046 // Convert to conditional expression

namespace DiveIndexPool;

/// <summary>
/// The <see cref="IndexPool"/> abstract class provides a base class for index pools
/// and the means to create index pools.
/// </summary>
public abstract class IndexPool
    : IIndexPool
{
    /// <inheritdoc/>
    public abstract Type IndexType { get; }

    /// <inheritdoc/>
    public virtual ulong Capacity { get; }

    /// <inheritdoc/>
    public abstract ulong Count { get; }

    /// <inheritdoc/>
    public virtual ulong AllocatedCount => Capacity - Count;

    /// <inheritdoc/>
    public abstract bool IsEmpty { get; }

    /// <inheritdoc/>
    public abstract bool IsThreadSafe { get; }

    /// <summary>
    /// Creates a new index pool with the given capacity and specifying whether or not
    /// the index pool should be thread safe.
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
    /// <param name="capacity">
    /// The capacity of the index pool. Index pools with capacities of 64 or less
    /// have special optimizations.
    /// </param>
    /// <param name="startIndex">
    /// Optional. The starting index for the index pool. Defaults to 0.
    /// </param>
    /// <param name="makeThreadSafe">
    /// Optional. Specify <see langword="true"/> to make the index pool thread safe.
    /// Defaults to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// A new index pool with the given capacity and thread safety.
    /// </returns>
    /// <exception cref="BackingTypeUnsupportedException">
    /// Thrown if the backing type <typeparamref name="T"/> is not a supported numeric primitive.
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
    public static IIndexPool<T> Create<T>(ulong capacity, T? startIndex = null,
                                          bool makeThreadSafe = false)
        where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
    {
        if (!Values<T>.IsSupportedBackingType)
            BackingTypeUnsupportedException.ThrowIfUnsupported<T>();

        //if (makeThreadSafe)
        //{
        //    return capacity <= DefaultSmallCapacity
        //        ? new ConcurrentSmallIndexPool<T>((byte)capacity, startIndex ?? T.Zero)
        //        : new ConcurrentSimpleIndexPool<T>(capacity, startIndex ?? T.Zero);
        //}

        return capacity <= DefaultSmallCapacity
               ? new SmallIndexPool<T>((byte)capacity, startIndex ?? T.Zero)
               : new LargeIndexPool<T>(capacity, startIndex ?? T.Zero);
    }

    /// <inheritdoc/>
    public abstract void Reset();

    /// <inheritdoc/>
    public abstract void Clear();
}

/// <summary>
/// The <see cref="IndexPool{T}"/> class provides the means to create index pools.
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
/// Thrown if the backing type <typeparamref name="T"/> is not a supported numeric primitive.
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
public abstract class IndexPool<T>
    : IndexPool, IIndexPool<T>
    where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Statically initializes the <see cref="IndexPool{T}"/> class.
    /// </summary>
    /// <exception cref="BackingTypeUnsupportedException">
    /// Thrown if the backing type <typeparamref name="T"/> is not a supported numeric primitive.
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
    static IndexPool()
    {
        BackingTypeUnsupportedException.ThrowIfUnsupported<T>();
    }

    /// <inheritdoc/>
    public abstract T StartIndex { get; }

    /// <inheritdoc/>
    public override Type IndexType => typeof(T);

    /// <inheritdoc/>
    public abstract bool IsValid(T index);

    /// <inheritdoc/>
    public abstract T Take();

    /// <inheritdoc/>
    public abstract T[] Take(ulong count);

    /// <inheritdoc/>
    public abstract bool TryTake(out T index);

    /// <inheritdoc/>
    public abstract T[] TakeAll();

    /// <inheritdoc/>
    public abstract bool TryTakeAll(out T[] indexes);

    /// <inheritdoc/>
    public abstract T Peek();

    /// <inheritdoc/>
    public abstract bool TryPeek(out T index);

    /// <inheritdoc/>
    public abstract bool Contains(T index);

    /// <inheritdoc/>
    public virtual bool IsAllocated(T index)
    {
        if (!IsValid(index))
            return false;

        return !Contains(index);
    }

    /// <inheritdoc/>
    public abstract bool Return(T index);

    /// <inheritdoc/>
    public abstract ulong ReturnAll(params ReadOnlySpan<T> indexes);

    /// <inheritdoc/>
    public abstract T[] ToArray();

    /// <inheritdoc/>
    public abstract IEnumerator<T> EnumerateAllocated();

    /// <inheritdoc/>
    public abstract IEnumerator<T> GetEnumerator();
}
