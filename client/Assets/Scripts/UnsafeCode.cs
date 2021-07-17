using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class UnsafeCode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static T ByteArrayToStructure<T>(byte[] data)
    {
        fixed (byte* ptr = data)
        {
            return Marshal.PtrToStructure<T>((IntPtr)ptr);
        }
    }
}
