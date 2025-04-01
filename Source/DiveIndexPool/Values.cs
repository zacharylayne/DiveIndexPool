using System.Numerics;

namespace DiveIndexPool;

/// <summary>
/// The <see cref="Values"/> class provides constant &amp; read-only values
/// for the <see cref="DiveIndexPool"/> library.
/// </summary>
public static class Values
{
    /// <summary>
    /// The list of all supported backing types for bit fields.
    /// </summary>
    public static readonly IReadOnlyList<Type> SupportedBackingTypes =
    [
        typeof(byte),
        typeof(char),
        typeof(int),
        typeof(long),
        typeof(nint),
        typeof(nuint),
        typeof(sbyte),
        typeof(short),
        typeof(uint),
        typeof(ulong),
        typeof(ushort),
    ];

    /// <summary>
    /// A map of types to the number of bits they can hold
    /// </summary>
    public static readonly IDictionary<Type, int> BitCount = new Dictionary<Type, int>()
    {
        { typeof(byte),      8 },
        { typeof(char),     16 },
        { typeof(int),      32 },
        { typeof(long),     64 },
        { typeof(nint),     IntPtr.Size },
        { typeof(nuint),    IntPtr.Size },
        { typeof(sbyte),     8 },
        { typeof(short),    16 },
        { typeof(uint),     32 },
        { typeof(ulong),    64 },
        { typeof(ushort),   16 },
    };

    /// <summary>
    /// The default capacity for an index pool (<c>64</c>).
    /// </summary>
    /// <seealso cref="DefaultMaxCapacity"/>
    /// <seealso cref="AbsoluteMaxCapacity"/>
    public static readonly ulong DefaultSmallCapacity = 64; // BitCount[typeof(ulong)];

    /// <summary>
    /// The default maximum capacity for an index pool (<c>1,073,741,824</c>).
    /// </summary>
    /// <seealso cref="DefaultSmallCapacity"/>
    /// <seealso cref="AbsoluteMaxCapacity"/>
    public static readonly ulong DefaultMaxCapacity = 1_073_741_824; // (ulong)(BitCount[typeof(ulong)] * 16);

    /// <summary>
    /// The absolute maximum capacity for an index pool (<c>137,438,953,408</c>).
    /// </summary>
    /// <seealso cref="DefaultSmallCapacity"/>
    /// <seealso cref="DefaultMaxCapacity"/>
    public static readonly ulong AbsoluteMaxCapacity = 137_438_953_408; // (ulong)(BitCount[typeof(ulong)] * Array.MaxLength);
}

/// <summary>
/// The <see cref="Values{T}"/> class provides for type <typeparamref name="T"/>
/// constant &amp; read-only values for the <see cref="DiveIndexPool"/> library.
/// </summary>
/// <typeparam name="T">
/// The type to provide values for.
/// </typeparam>
public static class Values<T>
    where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
{
    /// <summary>
    /// Statically initializes information about the specified backing type.
    /// </summary>
    static Values()
    {
        if (SupportedBackingTypes.Contains(typeof(T)))
        {
            MaxBitCount = BitCount[typeof(T)];
            IsSupportedBackingType = true;

            return;
        }

        MaxBitCount = 0;
        IsSupportedBackingType = false;
    }

    /// <summary>
    /// Whether or not <typeparamref name="T"/> is a supported type to back index pools.
    /// </summary>
    /// <value>
    /// A value of <see langword="true"/> if <typeparamref name="T"/> is a supported type;
    /// <see langword="false"/> otherwise.
    /// </value>
    /// <seealso cref="SupportedBackingTypes"/>
    public static readonly bool IsSupportedBackingType;

    /// <summary>
    /// The maximum number of bits the type <typeparamref name="T"/> can hold.
    /// </summary>
    /// <value>
    /// The maximum number of bits the type <typeparamref name="T"/> can hold.
    /// </value>
    public static readonly int MaxBitCount;

    /// <summary>
    /// The maximum value of <typeparamref name="T"/>
    /// (<see cref="MaxValue"/> - <see cref="One"/>).
    /// </summary>
    /// <value>
    /// The maximum value of <typeparamref name="T"/>.
    /// </value>
    /// <seealso cref="MinValue"/>
    /// <see href="https://learn.microsoft.com/dotnet/api/system.numerics.iminmaxvalue-1.maxvalue">
    /// System.Numerics.IMinMaxValue{T}.MaxValue</see>
    public static readonly T MaxValue = T.MaxValue - One;

    /// <summary>
    /// The minimum value of <typeparamref name="T"/>.
    /// </summary>
    /// <value>
    /// The minimum value of <typeparamref name="T"/>.
    /// </value>
    /// <seealso cref="MaxValue"/>
    /// <see href="https://learn.microsoft.com/dotnet/api/system.numerics.iminmaxvalue-1.minvalue">
    /// System.Numerics.IMinMaxValue{T}.MinValue</see>
    public static readonly T MinValue = T.MinValue;

    /// <summary>
    /// The value of <c>1</c> as <typeparamref name="T"/>.
    /// </summary>
    /// <value>
    /// The value of <c>1</c> as <typeparamref name="T"/>.
    /// </value>
    /// <seealso cref="Zero"/>
    /// <see href="https://learn.microsoft.com/dotnet/api/system.numerics.inumberbase-1.one">
    /// System.Numerics.INumberBase{T}.One</see>
    public static readonly T One = T.One;

    /// <summary>
    /// The value <c>0</c> as <typeparamref name="T"/>.
    /// </summary>
    /// <value>
    /// The value <c>0</c> as <typeparamref name="T"/>.
    /// </value>
    /// <seealso cref="One"/>
    /// <see href="https://learn.microsoft.com/dotnet/api/system.numerics.inumberbase-1.zero">
    /// System.Numerics.INumberBase{T}.Zero</see>
    public static readonly T Zero = T.Zero;

    /// <summary>
    /// The value used to represent an invalid index.
    /// </summary>
    public static readonly T InvalidIndex = T.AllBitsSet;
}
