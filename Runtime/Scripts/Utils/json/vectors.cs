using System;
using UnityEngine;

namespace VRWeb.Utils
{

	[Serializable]
    public struct SimpleVector3
    {
        public float x, y, z;

        public static implicit operator SimpleVector3( Vector3 vector )
        {
            SimpleVector3 simpleVector = new() { x = vector.x, y = vector.y, z = vector.z };
            return simpleVector;
        }

        public static implicit operator Vector3( SimpleVector3 vector )
        {
            return new(vector.x, vector.y, vector.z);
        }

        public static implicit operator string( SimpleVector3 vector )
        {
            return "< " + vector.x + ", " + vector.y + ", " + vector.z + " >";
        }
    }

    [Serializable]
    public struct SimpleQuaternion
    {
        public float x, y, z, w;

        public static implicit operator SimpleQuaternion( Quaternion vector )
        {
            SimpleQuaternion simpleVector = new()
            {
                x = vector.x, y = vector.y, z = vector.z, w = vector.w,
            };
            return simpleVector;
        }

        public static implicit operator Quaternion( SimpleQuaternion quat )
        {
            return new ( quat.x, quat.y, quat.z, quat.w );
        }
    }
}