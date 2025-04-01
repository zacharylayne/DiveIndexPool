using System.Numerics;

namespace DiveIndexPool;

// #TODO: Clone, CopyTo, TryCopyTo, CopyToAndResize, TryCopyToAndResize, Take(n), TryTake(n)
//        Reserve, TryReserve, ReserveAll, TryReserveAll, IsNearEmpty, RangeCount,
//        HasFreeIndexAbove, HasFreeIndexBelow, LastIndexAllocated, PeekBack, TryPeekBack,
//        LimitedEnumerate, PageEnumerate

/// <summary>
/// The <see cref="IIndexPool"/> interface provides basic functionality for managing a pool of indexes.
/// </summary>
public interface IIndexPool
{
    /// <summary>
    /// Gets the type of the indexes in the pool.
    /// </summary>
    /// <value>
    /// The type of the indexes in the pool.
    /// </value>
    Type IndexType { get; }

    /// <summary>
    /// Gets whether or not the pool is thread-safe.
    /// </summary>
    /// <value>
    /// A value of <see langword="true"/> if the pool is thread-safe;
    /// otherwise, <see langword="false"/>.
    /// </value>
    bool IsThreadSafe { get; }

    /// <summary>
    /// Gets the pool's capacity.
    /// </summary>
    /// <value>
    /// The pool's capacity.
    /// </value>
    ulong Capacity { get; }

    /// <summary>
    /// Gets the number of indexes in the pool.
    /// </summary>
    ulong Count { get; }

    /// <summary>
    /// Gets the number of indexes that have been taken <em>(i.e. allocated)</em> from the pool.
    /// </summary>
    /// <value>
    /// The number of indexes that have been taken from the pool.
    /// </value>
    ulong AllocatedCount { get; }

    /// <summary>
    /// Gets a value indicating whether the pool is empty.
    /// </summary>
    /// <value>
    /// A value of <see langword="true"/> if the index pool is empty;
    /// otherwise, <see langword="false"/>.
    /// </value>
    bool IsEmpty { get; }

    /// <summary>
    /// Resets the pool to its initial state.
    /// </summary>
    /// <seealso cref="Clear"/>
    void Reset();

    /// <summary>
    /// Clears the pool of all indexes.
    /// </summary>
    /// <seealso cref="Reset"/>
    void Clear();
}

/// <summary>
/// The <see cref="IIndexPool{T}"/> interface provides basic functionality for managing a pool of indexes
/// of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The numeric type of the indexes.
/// </typeparam>
public interface IIndexPool<T>
    : IIndexPool
    where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Gets the starting index value.
    /// </summary>
    T StartIndex { get; }

    /// <summary>
    /// Returns whether or not the specified index is within the pool's capacity.
    /// </summary>
    /// <param name="index">
    /// The index to check.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if the index is within the pool's capacity;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Valid indexes typically are <c>[startIndex..(startIndex + poolCapacity - 1)]</c>
    /// </remarks>
    bool IsValid(T index);

    /// <summary>
    /// Takes a single index from the pool.
    /// </summary>
    /// <returns>
    /// The index that was taken from the pool or, if the pool is empty, <see cref="Values{T}.InvalidIndex"/>.
    /// </returns>
    /// <seealso cref="Take(ulong)"/>
    /// <seealso cref="TryTake(out T)"/>
    T Take();

    /// <summary>
    /// Takes a specified number of indexes from the pool.
    /// </summary>
    /// <param name="count">
    /// The number of indexes to take. Values are clamped between 1 and the pool's capacity.
    /// </param>
    /// <returns>
    /// An array of indexes.If the index pool doesn't have enough free indexes to fill the array,
    /// the remaining array indexes will be set to <see cref="Values{T}.InvalidIndex"/>
    /// </returns>
    /// <seealso cref="Take()"/>
    /// <seealso cref="TryTake(out T)"/>
    T[] Take(ulong count);

    /// <summary>
    /// Attempts to take a single index from the pool.
    /// </summary>
    /// <param name="index">
    /// Contains an index from the pool or <see cref="Values{T}.InvalidIndex"/> if there is none.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if an index was successfully taken;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Take()"/>
    /// <seealso cref="Take(ulong)"/>
    bool TryTake(out T index);

    /// <summary>
    /// Takes all free indexes from the pool.
    /// </summary>
    /// <returns>
    /// An array containing all free indexes. If there are no free indexes, an empty array is returned.
    /// </returns>
    /// <seealso cref="Take()"/>
    /// <seealso cref="Take(ulong)"/>
    /// <seealso cref="TryTake(out T)"/>
    /// <seealso cref="TryTakeAll(out T[])"/>
    T[] TakeAll();

    /// <summary>
    /// Attempts to take all free indexes from the pool.
    /// </summary>
    /// <param name="indexes">
    /// The indexes taken from the pool, or an empty array if no indexes were available.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if all free indexes were successfully taken;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Take()"/>
    /// <seealso cref="Take(ulong)"/>
    /// <seealso cref="TryTake(out T)"/>
    /// <seealso cref="TakeAll"/>
    bool TryTakeAll(out T[] indexes);

    /// <summary>
    /// Peeks at the next available index without taking it.
    /// </summary>
    /// <returns>
    /// The next available index, or <see cref="Values{T}.InvalidIndex"/> if the next index is not available.
    /// </returns>
    /// <seealso cref="TryPeek(out T)"/>
    T Peek();

    /// <summary>
    /// Attempts to peek at the next available index without taking it.
    /// </summary>
    /// <param name="index">
    /// The next available index or, if the next index is not available, <see cref="Values{T}.InvalidIndex"/>.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if the next index is available;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="Peek()"/>
    bool TryPeek(out T index);

    /// <summary>
    /// Returns whether or not the pool contains the specified index.
    /// </summary>
    /// <param name="index">
    /// The index to check for.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if the pool contains the index;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method will return <see langword="false"/> if the index is outside the pool's capacity
    /// or otherwise invalid.
    /// </remarks>
    /// <seealso cref="IsAllocated(T)"/>
    /// <seealso cref="IsValid(T)"/>
    bool Contains(T index);

    /// <summary>
    /// Returns whether or not the specified index is allocated.
    /// </summary>
    /// <param name="index">
    /// The index to check.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if the index is allocated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method will return <see langword="false"/> if the index is outside the pool's capacity
    /// or otherwise invalid.
    /// </remarks>
    /// <seealso cref="Contains(T)"/>
    /// <seealso cref="IsValid(T)"/>
    bool IsAllocated(T index);

    /// <summary>
    /// Returns an index to the pool.
    /// </summary>
    /// <param name="index">
    /// The index to return.
    /// </param>
    /// <returns>
    /// A value of <see langword="true"/> if the index was successfully returned;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <seealso cref="ReturnAll(ReadOnlySpan{T})"/>
    bool Return(T index);

    /// <summary>
    /// Returns a range of indexes to the pool.
    /// </summary>
    /// <param name="indexes">
    /// A span of indexes to return.
    /// </param>
    /// <returns>
    /// The number of indexes successfully returned to the pool.
    /// </returns>
    /// <seealso cref="Return(T)"/>
    ulong ReturnAll(params ReadOnlySpan<T> indexes);

    /// <summary>
    /// Returns a snapshot of the index pool as an array.
    /// </summary>
    /// <returns>
    /// The indexes in the pool or an empty array if the pool is empty.
    /// If there are more indexes in the pool than can be returned in an array,
    /// the array will be truncated.
    /// </returns>
    T[] ToArray();

    /// <summary>
    /// Enumerates the allocated indexes in the pool.
    /// </summary>
    /// <returns>
    /// An enumerator that allows for iterating through the allocated indexes.
    /// </returns>
    /// <seealso cref="GetEnumerator"/>
    IEnumerator<T> EnumerateAllocated();

    /// <summary>
    /// Enumerates the free indexes in the pool.
    /// </summary>
    /// <returns>
    /// An enumerator that allows for iterating through the free indexes.
    /// </returns>
    /// <seealso cref="EnumerateAllocated"/>
    IEnumerator<T> GetEnumerator();
}
