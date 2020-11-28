using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Common.Code.Extensions;

namespace BackendTests.Common.Code.Helpers
{
    [TestClass]
    public class TimeSpanExtensionsTests
    {
        [DataRow(1, 1.0f)]
        [DataRow(19, 1.0f)]
        [DataRow(10, 10.0f)]
        [DataRow(70, 3.0f)]
        [DataRow(10, .1f)]
        [DataRow(11, .1f)]
        [DataTestMethod]
        public void TestMultipliedBy(long original, float multiplier)
        {
            float expected = original * multiplier;
            // Silly test but did actually catch a bug :)
            Assert.AreEqual(TimeSpan.FromSeconds(expected).Ticks, TimeSpan.FromSeconds(original).MultipliedBy(multiplier).Ticks, delta: 100);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expected).Ticks, TimeSpan.FromMilliseconds(original).MultipliedBy(multiplier).Ticks, delta: 100);
            Assert.AreEqual(TimeSpan.FromMinutes(expected).Ticks, TimeSpan.FromMinutes(original).MultipliedBy(multiplier).Ticks, delta: 100);
        }
    }
}
