using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class UnsafeCode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static T ByteArrayToStructure<T>(byte[] data)
    {
        int sz = data.Length;
        byte* ptr = stackalloc byte[sz];
        IntPtr ptr2 = (IntPtr)ptr;
        Marshal.Copy(data, 0, ptr2, sz);
        return Marshal.PtrToStructure<T>(ptr2);
    }
}
