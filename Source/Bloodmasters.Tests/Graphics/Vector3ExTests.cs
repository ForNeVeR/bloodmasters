using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;

namespace Bloodmasters.Tests.Graphics
{
    public class Vector3ExTests
    {
        private Matrix GetEmptyMatrix() => new Matrix();
        private Matrix GetMatrixWithNormalColomns1()
        {
            Matrix matrix = new Matrix();

            matrix.Column1 = new Vector4(4f, 3f, 2f, 6f);
            matrix.Column2 = new Vector4(4f, 5f, 2f, 7f);
            matrix.Column3 = new Vector4(3f, 1f, 2f, 6f);
            matrix.Column4 = new Vector4(2f, 6f, 1f, 9f);

            return matrix;
        }
        private Matrix GetMatrixWithNormalColomns2()
        {
            Matrix matrix = new Matrix();

            matrix.Column1 = new Vector4(6f, 2f, 1f, 3f);
            matrix.Column2 = new Vector4(4f, 3f, 3f, 4f);
            matrix.Column3 = new Vector4(5f, 3f, 1f, 4f);
            matrix.Column4 = new Vector4(2f, 3f, 8f, 6f);

            return matrix;
        }
        private Matrix GetMatrixWithNormalColomns3()
        {
            Matrix matrix = new Matrix();

            matrix.Column1 = new Vector4(7f, 5f, 3f, 6f);
            matrix.Column2 = new Vector4(3f, 4f, 5f, 9f);
            matrix.Column3 = new Vector4(2f, 1f, 2f, 3f);
            matrix.Column4 = new Vector4(4f, 5f, 6f, 7f);

            return matrix;
        }

        private Viewport GetNormalViewport()
        {
            Viewport viewport = new Viewport();

            viewport.Height = 7;
            viewport.Width = 4;
            viewport.X = 3;
            viewport.Y = 6;
            viewport.MaxDepth = 9f;
            viewport.MinDepth = 1f;

            return viewport;
        }

        /// <summary>
        /// In this test all data (matrix, Viewimport and Vector3) is zero
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AllDataZero_Success()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(0f), new Viewport(), GetEmptyMatrix(), GetEmptyMatrix(), GetEmptyMatrix());

            //Assert
            Assert.NotNull(result);
        }
        #region Test vector is zero
        [Fact]
        public async Task VectorIsZero_True()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(0f), new Viewport(), GetMatrixWithNormalColomns3(),
                GetMatrixWithNormalColomns2(), GetMatrixWithNormalColomns1());

            //Assert
            Assert.True(result.IsZero);
        }
        [Fact]
        public async Task VectorIsZero_False()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(0f), GetNormalViewport(), GetEmptyMatrix(), GetEmptyMatrix(), GetEmptyMatrix());

            //Assert
            Assert.False(result.IsZero);
        }
        #endregion
        #region Test vector is normalized
        [Fact]
        public async Task IsNormalizedWithEmptyData_False()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(0f), new Viewport(), new Matrix(), new Matrix(), new Matrix());

            //Assert
            Assert.False(result.IsNormalized);
        }
        [Fact]
        public async Task IsNormalizedWithNotEmptyData_False()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(3f), GetNormalViewport(), GetMatrixWithNormalColomns3(),
                GetMatrixWithNormalColomns2(), GetMatrixWithNormalColomns1());

            //Assert
            Assert.False(result.IsNormalized);
        }
        #endregion

        [Fact]
        public async Task NormalWithData_Success()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(4f), GetNormalViewport(), GetMatrixWithNormalColomns2(), GetMatrixWithNormalColomns1(), GetMatrixWithNormalColomns3());

            //Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task CorrectCalculationsWithNormalData_Success()
        {
            //Act
            var result = Vector3Ex.Project(new Vector3(4f), GetNormalViewport(), GetMatrixWithNormalColomns1(),
                GetMatrixWithNormalColomns2(), GetMatrixWithNormalColomns3());

            //Assert
            Assert.True(result.X == 6.634325f && result.Y == 6.068153f && result.Z == 6.3136373f);
        }
    }
}
