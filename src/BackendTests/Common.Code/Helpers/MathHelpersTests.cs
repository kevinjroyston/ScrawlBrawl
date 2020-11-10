using Common.Code.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BackendTests.Common.Code.Helpers
{
    [TestClass]
    public class MathHelpersTests
    {
        [DataRow(1.0, 2.0, 2.0, 3.0, DisplayName = "Test1")]
        [DataRow(1.0, -2.0, -1.0, 4.0, DisplayName = "Test2")]
        [DataTestMethod]
        public void Lerp(double a, double b, double t, double answer)
        {
            Assert.AreEqual(answer, MathHelpers.Lerp(a, b, t));
        }

        [DataRow(1.0, 2.5, 1.0, 2.5, DisplayName = "Test1")]
        [DataRow(-1.0, 1.0, 1.8, 1.0, DisplayName = "Test2")]
        [DataRow(2.0, 1.0, 1.0, 1.0, DisplayName = "Test3")]
        [DataRow(1.0, -2.0, 0.0, 1.0, DisplayName = "Test4")]
        [DataRow(1.0, 1.0, 0.4, 1.0, DisplayName = "Test5")]
        [DataRow(0.0, 1.0, 0.4, 0.4, DisplayName = "Test6")]
        [DataTestMethod]
        public void ClampedLerp(double a, double b, double t, double answer)
        {
            Assert.AreEqual(answer, MathHelpers.ClampedLerp(a, b, t));
        }
        // TODO add more tests
    }
}
