# DiveIndexPool

**DiveIndexPool** is a library for .NET 8.0 that provides performant, memory-efficient pools of integer indexes. Whether you need a small pool of up to 64 indexes or a pool large enough for thousands, **DiveIndexPool** offers simple, clear APIs for taking, returning, enumerating, and managing indexes.

## Features

* **Supports All Primitive Numeric Types**<br/>
  Create pools that manage indexes of any primitive numeric type: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `char`, `nint`, `nuint`

* **Optimized for Performance**<br/>
  Index pools rely on bitmasks for efficient allocation, deallocation, and enumeration, minimizing memory usage and maintaining performance. Additional optimizations are applied for small pools of up to 64 indexes.

* **Flexible API:**<br/>
  * Easily take, peek, return, and batch-operate on indexes.
  * Specify what index the pool should start at.
  * Check if indexes are valid, available, or allocated.
  * Enumerate both free & allocated indexes.
  * Convert pools to array.
  * Clear and reset pools fast.

* **No Exceptions, No Nulls, No Accidental Failure**<br/>
  * Methods return `InvalidIndex` values to indicate unavailable indexes rather than null.
  * Try-methods ensure no exceptions are thrown.
  * Invalid arguments are handled gracefully through clearly documented behavior.

> [!NOTE]  
> This library is currently in beta. Thread-safe pool types and additional features are planned in the very near future.

## Use Cases

- **Resource Handle Management:**  
  Efficiently manage a fixed set of resource handles (e.g., buffer slots, game entity IDs).

- **Memory or Object Pooling:**  
  Use indexes as lightweight references to pooled objects or memory blocks.

- **Concurrent Systems:**  
  Provide thread‑safe unique identifier allocation in high‑performance, multi‑threaded applications.

- **Real‑Time Applications:**  
  Ideal for scenarios (e.g., games or simulations) where low latency and high throughput are critical.

## Installation

DiveIndexPool is available as source code in this repository.

To include it in your project:

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/zacharylayne/DiveIndexPool.git
   ```

1. **Add to Your Solution:**
   - Include the project or the source files directly in your solution.

To install the DiveIndexPool library from NuGet, run the following command in the Package Manager Console:

```
dotnet add package DiveIndexPool
```

## Usage

Here's a quick example using `IndexPool<int>` to manage a pool of indexes:

```csharp
using DiveIndexPool;

// Create an index pool with a capacity of 100 indexes, starting at index 0.
var pool = IndexPool<int>.Create(100);

// Take an index from the pool.
int index = pool.Take();
Console.WriteLine($"Took index: {index}");

// Check if an index is available without removing it.
if (pool.TryPeek(out int peekedIndex))
    Console.WriteLine($"Next available index: {peekedIndex}");

// Return the index back to the pool.
pool.Return(index);
Console.WriteLine($"Returned index: {index}");

// Take multiple indexes at once.
int[] indexes = pool.Take(5);
Console.WriteLine("Took indexes: " + string.Join(", ", indexes));

// Convert the current pool of available indexes to an array.
int[] available = pool.ToArray();
Console.WriteLine("Available indexes: " + string.Join(", ", available));
```

## Contributing

Contributions, suggestions, and improvements are welcome! Please feel free to fork the repository and submit pull requests or open issues for bugs and feature requests.

## License

This project is licensed under the [MIT License](https://www.github.com/zacharylayne/diveindexpool/blob/master/license.txt).
