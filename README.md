# DiveIndexPool

DiveIndexPool is a .NET library for creating index pools, providing a performant way to allocate,
recycle, and manage numeric indexes efficiently, making it ideal for resource management,
memory pooling, high‑throughput systems, and more.

#### Table of Contents

* [**Introduction**](#introduction)
* [**Features**](#features)
* [**Use Cases**](#use-cases)
* [**Installation**](#installation)
* [**Usage**](#usage)
* [**API Overview**](#api-overview)
* [**Supported Types**](#supported-types)
* [**Questions or Comments?**](#questions-or-comments)

[**Release history**](https://github.com/zacharylayne/DiveIndexPool/blob/master/CHANGELOG.md)

[**Contributing**](https://github.com/zacharylayne/DiveIndexPool/blob/master/CONTRIBUTING.md)

[**License**](https://github.com/zacharylayne/DiveIndexPool/blob/master/LICENSE.txt)

<br>

> [!NOTE]
> Please be aware that the DiveIndexPool library is in active development. New features and improvements are being added, so stay tuned for updates!

## Features

Some of the key features of DiveIndexPool include:

* **Efficient Index Management:**
  Provides a fast and memory-efficient way to allocate and recycle indexes, minimizing fragmentation and overhead.

* **Generic Index Types:**
  Supports any unmanaged integral type that implements [IBinaryInteger](https://docs.microsoft.com/dotnet/api/system.numerics.ibinaryinteger) and [IMinMaxValue](https://docs.microsoft.com/dotnet/api/system.numerics.iminmaxvalue), including:
  * `byte`, `sbyte`
  * `short`, `ushort`
  * `int`, `uint`
  * `long`, `ulong`
  * `char`
  * `nint`, `nuint`

* **Flexible API:**
  Easily take, peek, return, and even batch‑operate on indexes. Supports enumeration and conversion
  to arrays.

* **Clear Documentation:**
   Well-documented API with XML comments to help you understand the usage and functionality of each
   method and property.

* **Modern C# Features:**
  Leverages C# 12 features like static abstract interface members to provide a robust and type‑safe API.

* **Lock‑Free & Thread‑Safe:** *(In progress)*
  Uses atomic operations (CAS loops) on a bitmask (backed by `ulong[]`) for minimal contention in
  multi‑threaded scenarios.

## Use Cases

* **Resource Handle Management:**
  Efficiently manage a fixed set of resource handles (e.g., buffer slots, game entity IDs).

* **Task Scheduling:**
  Assign unique task IDs in a job queue or scheduler for tracking and prioritization.

* **Memory or Object Pooling:**
  Use indexes as lightweight references to pooled objects or memory blocks.

* **Concurrent Systems:**
  Provide thread‑safe unique identifier allocation in high‑performance, multi‑threaded applications.

* **Real‑Time Applications:**
  Ideal for scenarios (e.g., games or simulations) where low latency and high throughput are critical.

* **Data Structures:**
  Provide unique node identifiers in graphs or trees for efficient traversal and management.

* **Event Handling:**
  Manage listener IDs in an event-driven system to ensure unique identification of subscribers.

* **And Others!**

## Installation

DiveIndexPool is available as source code in this repository.

To include it in your project:

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/zacharylayne/DiveIndexPool.git
   ```

1. **Add to Your Solution:**
   * Include the project or the source files directly in your solution.dddddddddd

## Usage

Here's a quick example using `IndexPool<int>` to manage a pool of indexes:

```csharp
using DiveIndexPool;

class Program
{
    static void Main()
    {
        // Create an index pool with a capacity of 100 indexes, starting at index 0.
        var pool = IndexPool<int>.Create(100);

        // Take an index from the pool.
        int index = pool.Take();
        Console.WriteLine($"Took index: {index}");

        // Check if an index is available without removing it.
        if (pool.TryPeek(out int peekedIndex))
        {
            Console.WriteLine($"Next available index: {peekedIndex}");
        }

        // Return the index back to the pool.
        pool.Return(index);
        Console.WriteLine($"Returned index: {index}");

        // Take multiple indexes at once.
        int[] indexes = pool.Take(5);
        Console.WriteLine("Took indexes: " + string.Join(", ", indexes));

        // Convert the current pool of available indexes to an array.
        int[] available = (int[])pool;
        Console.WriteLine("Available indexes: " + string.Join(", ", available));
    }
}
```

## API Overview

* **Allocation & Reuse:**
  * `Take()`, `TryTake(out T index)`: Atomically allocate an available index.
  * `Peek()`, `TryPeek(out T index)`: Look at the next available index without removing it.
  * `Return(T index)`, `ReturnAll(ReadOnlySpan<T> indexes)`: Return one or more indexes to the pool.

* **Batch Operations:**
  * `Take(T count)`: Allocate a batch of indexes in one operation.

* **State & Utilities:**
  * `Reset()`: Reset the pool to its initial state.
  * `Resize(T newCapacity)`: Create a new pool with increased capacity, preserving current state.
  * `Clone()`: Deep-clone the current pool.
  * `ToArray()`: Get an array snapshot of all available indexes.

* **Enumeration:**
  Implements `IEnumerable<T>` to easily iterate over available indexes.

## Supported Types

DiveIndexPool supports all common integral types as long as they meet the constraints (`unmanaged`,
`IBinaryInteger<T>`, `IMinMaxValue<T>`). Please ensure that the capacity and starting index are within
the valid range for the chosen type.

## Questions or comments?

Feel free to submit an issue or add to the discussion! Feedback, suggestions, & collaboration are
more than welcome if you're interested in contributing to the project. Thanks for your interests in DiveIndexPool!