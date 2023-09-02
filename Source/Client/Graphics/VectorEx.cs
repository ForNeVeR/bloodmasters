using SharpDX;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class VectorEx
{
    public static Vector3 Project(this Vector3 vector, Viewport viewport, Matrix projection, Matrix view, Matrix world)
    {
        var worldViewProjection = world * view * projection;
        return Vector3.Project(
            vector,
            viewport.X,
            viewport.Y,
            viewport.Width,
            viewport.Height,
            viewport.MinDepth,
            viewport.MaxDepth,
            worldViewProjection);
    }

    public static Vector3 Unproject(
        this Vector3 vector,
        Viewport viewport,
        Matrix projection,
        Matrix view,
        Matrix world)
    {
        var worldViewProjection = world * view * projection;
        return Vector3.Unproject(
            vector,
            viewport.X,
            viewport.Y,
            viewport.Width,
            viewport.Height,
            viewport.MinDepth,
            viewport.MaxDepth,
            worldViewProjection);
    }

    // Conversion to Vector3
    public static Vector3 ToDx(this Vector3D a)
    {
        return new Vector3(a.x, a.y, a.z);
    }

    // Constructor
    public static Vector3D FromDx(this Vector3 v)
    {
        return new Vector3D(v.X, v.Y, v.Z);
    }
}
