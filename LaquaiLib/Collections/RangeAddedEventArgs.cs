﻿#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace LaquaiLib.Collections;

/// <summary>
/// Represents the event arguments for the RangeAdded event.
/// </summary>
/// <typeparam name="T">The type of the items being added.</typeparam>
public class RangeAddedEventArgs<T>
{
    /// <summary>
    /// Gets the items that were added.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeAddedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="items">The items that were added.</param>
    public RangeAddedEventArgs(IEnumerable<T> items)
    {
        Items = items;
    }
}
