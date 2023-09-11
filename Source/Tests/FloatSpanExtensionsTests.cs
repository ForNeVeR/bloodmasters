using CodeImp.Bloodmasters.Client.Extensions;

namespace Bloodmasters.Tests;

public class FloatSpanExtensionsTests
{
    [Theory(DisplayName = "Vectorized multiplication of float array should produce same result as scalar multiplication")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(16)]
    [InlineData(100)]
    [InlineData(1720)]
    [InlineData(1123456)]
    public void VectorizedMultiplicationOfFloatArrayShouldProduceSameResultAsScalarMultiplication(int arraySize)
    {
        var array1 = Enumerable.Range(0, arraySize).Select(i => (float)i).ToArray();
        var array2 = Enumerable.Range(0, arraySize).Select(i => (float)i).ToArray();

        const float scalarValue = 2f;

        array1.AsSpan().MultiplyByScalar(scalarValue);

        for (int i = 0; i < array2.Length; i++)
        {
            array2[i] *= scalarValue;
        }

        Assert.Equal(array1, array2);
    }
}
