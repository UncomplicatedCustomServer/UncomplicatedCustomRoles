using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class Vector3Extension
    {
        public static Vector3 AddY(this Vector3 vector, float value)
        {
            vector.y += value;
            return vector;
        }

        public static Vector3 AddX(this Vector3 vector, float value)
        {
            vector.x += value; 
            return vector;
        }

        public static Vector3 AddZ(this Vector3 vector, float value)
        {
            vector.z += value;
            return vector;
        }
    }
}
