// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;

namespace Autofac.Extras.AggregateService.SourceGenerator;

/// <summary>
/// An immutable, value-equatable wrapper around an array. Incremental generator
/// models must implement structural equality so Roslyn can cache pipeline
/// outputs; the default array reference equality would defeat that, so
/// collection-typed model members use this type.
/// </summary>
/// <typeparam name="T">
/// The type of element stored in the array.
/// </typeparam>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EquatableArray{T}"/> struct.
    /// </summary>
    /// <param name="array">
    /// The array to wrap.
    /// </param>
    public EquatableArray(T[] array)
    {
        _array = array;
    }

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    public int Count => _array?.Length ?? 0;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the element to get.
    /// </param>
    /// <returns>
    /// The element at the specified index.
    /// </returns>
    public T this[int index] => _array![index];

    /// <summary>
    /// Determines whether two <see cref="EquatableArray{T}"/> values are equal.
    /// </summary>
    /// <param name="left">
    /// The left value.
    /// </param>
    /// <param name="right">
    /// The right value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the values are equal; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="EquatableArray{T}"/> values are not
    /// equal.
    /// </summary>
    /// <param name="left">
    /// The left value.
    /// </param>
    /// <param name="right">
    /// The right value.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the values are not equal; otherwise
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    /// <inheritdoc/>
    public bool Equals(EquatableArray<T> other)
    {
        if (_array is null || other._array is null)
        {
            return _array is null && other._array is null;
        }

        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (var i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_array is null)
        {
            return 0;
        }

        var hash = 17;
        foreach (var item in _array)
        {
            hash = (hash * 31) + (item?.GetHashCode() ?? 0);
        }

        return hash;
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        var array = _array ?? Array.Empty<T>();
        foreach (var item in array)
        {
            yield return item;
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
