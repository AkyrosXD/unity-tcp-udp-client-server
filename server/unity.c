#include "unity.h"

Vector3 Vector3_right()
{
    Vector3 result = { 1.0f, 0, 0 };
    return result;
}

Vector3 Vector3_forward()
{
    Vector3 result = { 0, 0, 1.0f };
    return result;
}

Vector3 Vector3_Multiply(Vector3 a, float b)
{
    Vector3 result = { a.x * b, a.y * b, a.z * b };
    return result;
}

Vector3 Vector3_Addition(Vector3 a, Vector3 b)
{
    Vector3 result = { a.x + b.x, a.y + b.y, a.z + b.z };
    return result;
}

Vector3 Quaternion_Multiply(Quaternion rotation, Vector3 point)
{
    //Source: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Quaternion.cs
    
    float x = rotation.x * 2.0f;
    float y = rotation.y * 2.0f;
    float z = rotation.z * 2.0f;
    float xx = rotation.x * x;
    float yy = rotation.y * y;
    float zz = rotation.z * z;
    float xy = rotation.x * y;
    float xz = rotation.x * z;
    float yz = rotation.y * z;
    float wx = rotation.w * x;
    float wy = rotation.w * y;
    float wz = rotation.w * z;
    Vector3 res;
    res.x = (1.0f - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z;
    res.y = (xy + wz) * point.x + (1.0f - (xx + zz)) * point.y + (yz - wx) * point.z;
    res.z = (xz - wy) * point.x + (yz + wx) * point.y + (1.0f - (xx + yy)) * point.z;
    return res;
}