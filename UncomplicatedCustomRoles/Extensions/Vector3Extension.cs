/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using UncomplicatedCustomRoles.API.Struct;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class Vector3Extension
    {
        /// <summary>
        /// Adds a X value to the current <see cref="Vector3"/>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>The modificed <see cref="Vector3"/></returns>
        public static Vector3 AddX(this Vector3 vector, float value)
        {
            vector.x += value; 
            return vector;
        }

        /// <summary>
        /// Adds a Y value to the current <see cref="Vector3"/>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>The modificed <see cref="Vector3"/></returns>
        public static Vector3 AddY(this Vector3 vector, float value)
        {
            vector.y += value;
            return vector;
        }

        /// <summary>
        /// Adds a Z value to the current <see cref="Vector3"/>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <returns>The modificed <see cref="Vector3"/></returns>
        public static Vector3 AddZ(this Vector3 vector, float value)
        {
            vector.z += value;
            return vector;
        }

        /// <summary>
        /// Converts the current <see cref="Vector3"/> to a local <see cref="Triplet{TFirst, TSecond, TThird}}"/>
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Triplet<float, float, float> ToTriplet(this Vector3 vector)
        {
            return new(vector.x, vector.y, vector.z);
        }
    }
}
