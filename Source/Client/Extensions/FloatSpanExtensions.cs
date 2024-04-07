using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Bloodmasters.Client.Extensions;

public static class FloatSpanExtensions
{
    public static unsafe void MultiplyByScalar(this Span<float> span, float value)
    {
        if (Vector.IsHardwareAccelerated && span.Length >= Vector<float>.Count)
        {
            fixed (float* arrayPtr = &MemoryMarshal.GetReference(span))
            {
                float* current = arrayPtr;
                float* end = arrayPtr + span.Length - span.Length % Vector<float>.Count;
                do
                {
                    Vector<float> result = Vector.Multiply(*(Vector<float>*)current, value);
                    result.CopyTo(new Span<float>(current, Vector<float>.Count));

                    current += Vector<float>.Count;
                }
                while (current < end);

                // Scalar remainder loop
                for (int i = span.Length - span.Length % Vector<float>.Count; i < span.Length; i++)
                {
                    span[i] *= value;
                }
            }
        }
        // Scalar fallback path
        else
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] *= value;
            }
        }
    }
}
