using System;
using System.Numerics;
using Vortice.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class Direct3DDeviceEx
{
    public static unsafe void DrawUserPrimitives<T>(
        this IDirect3DDevice9 device,
        PrimitiveType primitiveType,
        int primitiveCount,
        T[] vertexStreamZeroData) where T : unmanaged
    {
        fixed (T* ptr = vertexStreamZeroData)
        {
            device.DrawPrimitiveUP(
                primitiveType,
                primitiveCount,
                (IntPtr)ptr,
                sizeof(T));
        }
    }
}
