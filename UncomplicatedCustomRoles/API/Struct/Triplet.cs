/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Text.Json.Serialization;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Struct;

public readonly struct Triplet<TFirst, TSecond, TThird>
{
    /// <summary>
    ///     Gets the first value
    /// </summary>
    public TFirst First { get; }

    /// <summary>
    ///     Gets the second value
    /// </summary>
    public TSecond Second { get; }

    /// <summary>
    ///     Gets the third value
    /// </summary>
    public TThird Third { get; }

    [JsonConstructor]
    public Triplet(TFirst first, TSecond second, TThird third)
    {
        First = first;
        Second = second;
        Third = third;
    }

    public Triplet(Triplet<TFirst, TSecond, TThird> clone)
    {
        First = clone.First;
        Second = clone.Second;
        Third = clone.Third;
    }

    public override string ToString()
    {
        return $"({First}, {Second}, {Third})";
    }

    public static Triplet<float, float, float> FromVector3(Vector3 vector)
    {
        return new Triplet<float, float, float>(vector.x, vector.y, vector.z);
    }
}