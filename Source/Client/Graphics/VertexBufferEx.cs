using System;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class VertexBufferEx
{
    public static unsafe Span<T> Lock<T>(
        this VertexBuffer vertexBuffer,
        int offsetToLock,
        int itemCount) where T : unmanaged
    {
        var sizeToLock = itemCount * sizeof(T);
        return new Span<T>((void*)vertexBuffer.LockToPointer(offsetToLock, sizeToLock, LockFlags.None), itemCount);
    }
}
