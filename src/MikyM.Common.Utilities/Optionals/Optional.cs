//
//  Optional.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MikyM.Common.Utilities.Optionals;

/// <summary>
/// Represents an optional value. This is mainly used for JSON de/serializalization where a value can be either
/// present, null, or completely missing.
///
/// While a <see cref="Nullable"/> allows for a value to be logically present but contain a null value,
/// <see cref="Optional{TValue}"/> allows for a value to be logically missing. For example, an optional without a
/// value would never be serialized, but a nullable with a null value would (albeit as "null").
/// </summary>
/// <typeparam name="TValue">The inner type.</typeparam>
public readonly struct Optional<TValue> : IOptional
{
    private readonly TValue _value;

    /// <summary>
    /// Gets an initialized instance of <see cref="Optional{T}"/> which has no value set.
    /// </summary>
    public static Optional<TValue> Default { get; } = new();

    /// <summary>
    /// Gets the value contained in the optional.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the optional does not contain a value.</exception>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TValue Value
    {
        get
        {
            if (this.HasValue)
            {
                return _value;
            }

            throw new InvalidOperationException("The optional did not contain a valid value.");
        }
    }

    /// <inheritdoc />
    public bool HasValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{TValue}"/> struct.
    /// </summary>
    /// <param name="value">The contained value.</param>
    public Optional(TValue value)
    {
        _value = value;
        this.HasValue = true;
    }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(true, nameof(_value))]
    public bool IsDefined() => this.HasValue && _value is not null;

    /// <summary>
    /// Determines whether the option has a defined value; that is, whether it both has a value and that value is
    /// non-null.
    /// </summary>
    /// <param name="value">The defined value.</param>
    /// <returns>true if the optional has a value and that value is non-null; otherwise, false.</returns>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsDefined([NotNullWhen(true)] out TValue? value)
    {
        value = default;

        if (!IsDefined())
        {
            return false;
        }

        value = _value;
        return true;
    }

    /// <summary>
    /// Implicitly converts actual values into an optional.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The created optional.</returns>
    public static implicit operator Optional<TValue>(TValue value)
    {
        return new(value);
    }

    /// <summary>
    /// Compares two optionals, for equality.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>true if the operands are equal, false otherwise.</returns>
    public static bool operator ==(Optional<TValue> left, Optional<TValue> right)
        => left.Equals(right);

    /// <summary>
    /// Compares two optionals, for inequality.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>false if the operands are equal, true otherwise.</returns>
    public static bool operator !=(Optional<TValue> left, Optional<TValue> right)
        => !left.Equals(right);

    /// <summary>
    /// Compares this instance for equality with another instance.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns>true if the instances are considered equal; otherwise, false.</returns>
    public bool Equals(Optional<TValue> other)
    {
        return EqualityComparer<TValue>.Default.Equals(_value, other._value) && this.HasValue == other.HasValue;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Optional<TValue> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_value, this.HasValue);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.HasValue
            ? $"{{{_value?.ToString() ?? "null"}}}"
            : "Empty";
    }
}

/// <summary>
/// Utilities for creation of optional properties.
/// </summary>
public static class Optional
{
    /// <summary>
    /// Creates a new <see cref="Optional{T}"/> from a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value to create an optional property for.</typeparam>
    /// <param name="value">Value to set the property to.</param>
    /// <returns>Created optional property, which has a specified value set.</returns>
    public static Optional<T> FromValue<T>(T value)
        => new(value);

    /// <summary>
    /// Creates a new <see cref="Optional{T}"/> from a default value for type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value to create an optional property for.</typeparam>
    /// <returns>Created optional property, which has a default value for <typeparamref name="T"/> set.</returns>
    public static Optional<T?> FromDefaultValue<T>()
        => new(default);

    /// <summary>
    /// Creates a new <see cref="Optional{T}"/> which has no value.
    /// </summary>
    /// <typeparam name="T">Type of the value to create an optional property for.</typeparam>
    /// <returns>Created optional property, which has no value set.</returns>
    public static Optional<T> FromNoValue<T>()
        => Optional<T>.Default;
}