using System.Numerics;

namespace DiveIndexPool;

/// <summary>
/// The <see cref="BackingTypeUnsupportedException"/> class is thrown when an
/// unsupported backing type is used to create a bit field.
/// </summary>
/// <remarks>
/// To throw this exception, use the method <see cref="ThrowIfUnsupported{T}"/>.
/// </remarks>
public class BackingTypeUnsupportedException
    : Exception
{
    /// <summary>
    /// Creates a new <see cref="BackingTypeUnsupportedException"/> instance.
    /// </summary>
    /// <param name="type">
    /// The backing type that is not supported.
    /// </param>
    /// <seealso cref="BackingTypeUnsupportedException(Type, string)"/>
    /// <seealso cref="BackingTypeUnsupportedException()"/>
    /// <seealso cref="BackingTypeUnsupportedException(string)"/>
    /// <seealso cref="BackingTypeUnsupportedException(string, Exception)"/>
    public BackingTypeUnsupportedException(Type type)
        : base($"The backing type '{type}' is not supported.") { }

    /// <summary>
    /// Creates a new <see cref="BackingTypeUnsupportedException"/> instance.
    /// </summary>
    /// <param name="type">
    /// The backing type that is not supported.
    /// </param>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// </param>
    /// <seealso cref="BackingTypeUnsupportedException(Type)"/>
    /// <seealso cref="BackingTypeUnsupportedException()"/>
    /// <seealso cref="BackingTypeUnsupportedException(string)"/>
    /// <seealso cref="BackingTypeUnsupportedException(string, Exception)"/>
    public BackingTypeUnsupportedException(Type type, string message)
        : base($"Type: {type} {message}") { }

    /// <summary>
    /// Creates a new <see cref="BackingTypeUnsupportedException"/> instance.
    /// </summary>
    /// <seealso cref="BackingTypeUnsupportedException(Type)"/>
    /// <seealso cref="BackingTypeUnsupportedException(Type, string)"/>
    /// <seealso cref="BackingTypeUnsupportedException(string)"/>
    /// <seealso cref="BackingTypeUnsupportedException(string, Exception)"/>
    public BackingTypeUnsupportedException()
        : base("The backing type is not supported.") { }

    /// <summary>
    /// Creates a new <see cref="BackingTypeUnsupportedException"/> instance.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// </param>
    /// <seealso cref="BackingTypeUnsupportedException(Type)"/>
    /// <seealso cref="BackingTypeUnsupportedException(Type, string)"/>
    /// <seealso cref="BackingTypeUnsupportedException()"/>
    /// <seealso cref="BackingTypeUnsupportedException(string, Exception)"/>
    public BackingTypeUnsupportedException(string message)
        : base(message) { }

    /// <summary>
    /// Creates a new <see cref="BackingTypeUnsupportedException"/> instance.
    /// </summary>
    /// <param name="message">
    /// The error message that explains the reason for the exception.
    /// </param>
    /// <param name="inner">
    /// The exception that is the cause of the current exception, or a null reference
    /// </param>
    /// <seealso cref="BackingTypeUnsupportedException(Type)"/>
    /// <seealso cref="BackingTypeUnsupportedException(Type, string)"/>
    /// <seealso cref="BackingTypeUnsupportedException()"/>
    /// <seealso cref="BackingTypeUnsupportedException(string)"/>
    public BackingTypeUnsupportedException(string message, Exception inner)
        : base(message, inner) { }

    /// <summary>
    /// Throws <see cref="BackingTypeUnsupportedException"/> if the type <typeparamref name="T"/>
    /// is not a supported backing type for a bit field.
    /// </summary>
    /// <typeparam name="T">
    /// The type to check.
    /// </typeparam>
    /// <exception cref="BackingTypeUnsupportedException">
    /// Thrown when the specified type is not supported as a backing type for a bit field.
    /// </exception>
    public static void ThrowIfUnsupported<T>()
        where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
    {
        if (!SupportedBackingTypes.Contains(typeof(T)))
            throw new BackingTypeUnsupportedException(typeof(T));
    }
}
