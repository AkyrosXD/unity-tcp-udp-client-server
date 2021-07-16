#ifndef UNITY_H

#define FIXED_DELTA_TIME 0.033333f

typedef struct Vector3
{
    float x;
    float y;
    float z;
} Vector3;

typedef struct Quaternion
{
    float x;
    float y;
    float z;
    float w;
} Quaternion;

Vector3 Vector3_right();
Vector3 Vector3_forward();
Vector3 Vector3_Multiply(Vector3 a, float b);
Vector3 Vector3_Addition(Vector3 a, Vector3 b);
Vector3 Quaternion_Multiply(Quaternion rotation, Vector3 point);

#define UNITY_H
#endif