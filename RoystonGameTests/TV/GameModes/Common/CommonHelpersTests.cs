using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGameTests.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace RoystonGameTests.TV.GameModes.Common
{
    [TestClass]
    public class CommonHelpersTests
    {
        [DataRow(10, 5, 0.0, 1.0, 0.0, 10)]
        [DataRow(10, 5, 1.0, 2.0, 0.5, 10)]
        [DataRow(10, 5, 1.0, 2.0, 2.0, 5)]
        [DataRow(200, 10, 1.0, 2.0, 2.1, 10)]
        [DataRow(100, 50, 1.0, 3.0, 2.0, 75)]
        [DataRow(100, 1, 0.0, 1.0, 0.2, 80)]
        [DataTestMethod]
        public void PointsForSpeedTest(int maxPoints, int minPoints, double startTime, double endTime, double secondsTaken, int? expectedValue)
        {
            if (expectedValue.HasValue)
            {
                Assert.AreEqual(expectedValue, CommonHelpers.PointsForSpeed(maxPoints, minPoints, startTime, endTime, secondsTaken));
            }
            else
            {
                // Testing it does not error, do not care about return value.
                CommonHelpers.PointsForSpeed(maxPoints, minPoints, startTime, endTime, secondsTaken);
            }
        }

        [DataRow(
            new int[] { 1, 1, 1, 2, 2, 3},
            5,
            new int[] { 1, 1, 2, 2, 3})]
        [DataRow(
            new int[] { 1, 1, 1, 2, 2, 3 },
            10,
            new int[] { 1, 1, 1, 2, 2, 3 })]
        [DataRow(
            new int[] { 1, 1, 1, 2, 2, 3 },
            1,
            new int[] { 3 })]
        [DataRow(
            new int[] { 1, 1, 2, 2, 2, 3 },
            4,
            new int[] { 1, 2, 2, 3 })]
        [DataRow(
            new int[] { 1, 1, 2, 2, 3, 3 },
            5,
            new int[] { 1, 2, 2, 3, 3 })]
        [DataTestMethod]
        public void TrimUserInputListTest(int[] inputIds, int numInputsWanted, int[] expectedIds)
        {
            TestUserManager.ResetTestUsers();
            List<UserCreatedObject> inputObjects = inputIds.Select(id => new UserCreatedObject() { Owner = TestUserManager.GetTestUser(id) }).ToList();
            List<UserCreatedObject> expectedObjects = expectedIds.Select(id => new UserCreatedObject() { Owner = TestUserManager.GetTestUser(id) }).ToList();

            List<UserCreatedObject> returnedObjects = CommonHelpers.TrimUserInputList(userInputs: inputObjects, numInputsWanted: numInputsWanted).ToList();
            Assert.IsTrue(returnedObjects.HasSameElements(expectedObjects));
        }
    }
}
