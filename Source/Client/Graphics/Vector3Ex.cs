using SharpDX;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class Vector3Ex
{
    public static Vector3 Project(this Vector3 vector, Viewport viewport, Matrix projection, Matrix view, Matrix world)
    {
        // TODO[#15]: Verify this combination!
        var worldViewProjection = Matrix.Multiply(Matrix.Multiply(world, view), projection);
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
        // TODO[#15]: Verify this combination!
        var worldViewProjection = Matrix.Multiply(Matrix.Multiply(world, view), projection);
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
}
