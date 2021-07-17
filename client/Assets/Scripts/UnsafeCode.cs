using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class UnsafeCode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ByteArrayToStructure<T>(byte[] data)
    {
        fixed (byte* ptr = data)
        {
            return Marshal.PtrToStructure<T>((IntPtr)ptr);
        }
    }

    // same as memcpy in C
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void* Memcpy(void* dest, void* src, int len)
    {
        sbyte* d = (sbyte*)dest;
        sbyte* s = (sbyte*)src;
        while (len-- > 0)
            *d++ = *s++;
        return dest;
    }

    // since we cant do ptr + offset in like we can do in C,
    // we have to come out with another way
    public static byte[] SubArray(byte[] array, int offset, int length)
    {
        byte[] result = new byte[length];
        fixed (byte* ptr = array, ptrResult = result)
        {
            byte* newPtr = ptr + offset;
            Memcpy(ptrResult, newPtr, length);
        }
        return result;
    }
}
