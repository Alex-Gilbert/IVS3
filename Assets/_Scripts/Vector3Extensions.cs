using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 StereographicProjection(this Vector4 v4)
    {
        if (v4.w == 1)
        {
            return Vector3.one * 100;
        }
        return new Vector3(v4.x / (v4.w - 1), v4.y / (v4.w - 1), v4.z / (v4.w - 1));
    }

    public static Vector3 StereographicProjection(this Quaternion quat)
    {
        if (quat.w == 1)
        {
            return Vector3.one * 100;
        }
        return new Vector3(quat.x / (quat.w - 1), quat.y / (quat.w - 1), quat.z / (quat.w - 1));
    }

    public static Quaternion NormailzeQuaternion(this Quaternion quat)
    {
        var normal = new Vector4(quat.x, quat.y, quat.z, quat.w);
        normal.Normalize();
        return new Quaternion(normal.x, normal.y, normal.z, normal.w);
    }
}
