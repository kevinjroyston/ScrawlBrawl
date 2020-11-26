using Backend.Games.Common.DataModels;
using BackendTests.TV.DataModels;
using Common.Code.Extensions;
using Common.Code.Helpers;
using Common.DataModels.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackendTests.Common.Code.Helpers
{
    [TestClass]
    public class MemberHelpersTests
    {
        [DataRow(
            new int[] { 1, 1, 1, 2, 2, 3 },
            5,
            new int[] { 1, 1, 2, 2, 3 })]
        [DataRow(
            new int[] { 1, 1, 1, 2, 2, 3 },
            10,
            new int[] { 1, 1, 1, 2, 2, 3 })]
        [DataRow(
            new int[] { 1, 1, 1, 2, 2, 3 },
            3,
            new int[] { 1, 2, 3 })]
        [DataRow(
            new int[] { 1, 2, 2, 2, 2, 2, 2, 2, 2, 3 },
            5,
            new int[] { 1, 2, 2, 2, 3 })]
        [DataRow(
            new int[] { 1, 1, 2, 2, 3, 3 },
            3,
            new int[] { 1, 2, 3 })]
        [DataTestMethod]
        public void SelectOrdered(int[] inputIds, int numInputsWanted, int[] expectedIds)
        {
            TestUserManager.ResetTestUsers();
            List<UserCreatedObject> inputObjects = inputIds.Select(id => new UserCreatedObject() { Owner = TestUserManager.GetTestUser(id), Id = TestUserManager.GetTestUser(id).Id }).ToList();
            List<UserCreatedObject> expectedObjects = expectedIds.Select(id => new UserCreatedObject() { Owner = TestUserManager.GetTestUser(id), Id = TestUserManager.GetTestUser(id).Id }).ToList();

            List<UserCreatedObject> returnedObjects = MemberHelpers<UserCreatedObject>.Select_Ordered(inputObjects, numInputsWanted).ToList();
            Assert.IsTrue(returnedObjects.HasSameElements(expectedObjects), $"Expected:{string.Join(",", expectedObjects.Select(obj => obj.Owner.DisplayName))} Actual:{string.Join(",", returnedObjects.Select(obj => obj.Owner.DisplayName))}");
        }

        [TestMethod]
        public void SelectOrdered_RandomlyGeneratedCases()
        {
            Random rand = new Random(Seed: 101);
            const int generatedTestCaseCount = 1000;

            for (int _ = 0; _ < generatedTestCaseCount; _++)
            {
                TestUserManager.ResetTestUsers();

                // Test should work with any values (undefined behavior for negatives)
                int numUsers = rand.Next(3, 50);
                int numInputsWanted = (numUsers * rand.Next(0, 5)) + rand.Next(1, 50);

                Dictionary<Guid, List<UserCreatedObject>> userIdToObjectList = new Dictionary<Guid, List<UserCreatedObject>>();

                for (int userId = 0; userId < numUsers; userId++)
                {
                    // Based on implementation of SelectOrdered, 0 members is equivalent to numUsers--.
                    int numMembersForUser = rand.Next(1, 8);
                    userIdToObjectList[TestUserManager.GetTestUser(userId).Id] = new List<UserCreatedObject>();
                    for (int objIter = 0; objIter < numMembersForUser; objIter++)
                    {
                        userIdToObjectList[TestUserManager.GetTestUser(userId).Id].Add(
                            new UserCreatedObject()
                            {
                                Owner = TestUserManager.GetTestUser(userId),
                                Id = Guid.NewGuid()
                            });
                    }
                }
                List<UserCreatedObject> inputObjects = userIdToObjectList.Values.SelectMany(val => val).ToList();
                List<UserCreatedObject> returnedObjects = MemberHelpers<UserCreatedObject>.Select_Ordered(
                    inputObjects,
                    numInputsWanted).ToList();

                if (inputObjects.Count <= numInputsWanted)
                {
                    Assert.AreEqual(inputObjects.Count, returnedObjects.Count);
                    Assert.IsTrue(returnedObjects.HasSameElements(inputObjects));
                    continue;
                }

                Assert.AreEqual(numInputsWanted, returnedObjects.Count);

                Dictionary<Guid, List<UserCreatedObject>> groupedReturnedObjects = userIdToObjectList.Keys.Select(
                    (userId) =>
                        (userId,
                        returnedObjects.Where((member) => member.Source == userId).ToList()))
                    .ToDictionary((val) => val.Item1, (val) => val.Item2);

                int largestGroup = groupedReturnedObjects.Values.Max((val) => val.Count);
                Assert.IsTrue(groupedReturnedObjects.All((val) => (val.Value.Count >= largestGroup - 1) || (userIdToObjectList[val.Key].Count == val.Value.Count)));
                Assert.IsTrue(groupedReturnedObjects.All((val) => (val.Value.Zip(userIdToObjectList[val.Key].Take(val.Value.Count)).All((pair)=> pair.First?.Id == pair.Second?.Id))));
            }
        }

        // TODO test dynamic weighted random.
        // TODO add more test cases for assignment

        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, 3, 5, 10, false)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, 10, 5, 10, true)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, 3, 5, 4, false)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 10, 12, 10, false)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6 }, 12, 6, 12, true)]
        [DataRow(new int[] { 1, 2, 3 }, 3, 6, 10, false)]
        [DataRow(new int[] { 1, 2, 3, 4, 5, 6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25 }, 100, 5, 10, true)]
        [DataTestMethod]
        public void Assign(int[] inputIds, int duplicateCount, int groups, int maxPerGroup, bool allowDuplicateIds)
        {
            const int rerunCount = 100;
            Exception exc = null;
            int failCount = 0;
            for (int i = 0; i< rerunCount; i++)
            {
                try
                {

                    TestUserManager.ResetTestUsers();
                    List<IConstraints<UserCreatedObject>> constraints = Enumerable.Range(0, groups)
                        .Select(_ => new Constraints<UserCreatedObject>
                        {
                            AllowDuplicateIds = allowDuplicateIds,
                            MaxMemberCount = maxPerGroup,
                        }).Cast<IConstraints<UserCreatedObject>>().ToList();
                    List<UserCreatedObject> inputObjects = inputIds.Select(id => new UserCreatedObject() { Owner = TestUserManager.GetTestUser(id), Id = TestUserManager.GetTestUser(id).Id }).ToList();

                    List<IGroup<UserCreatedObject>> returnedObjects = MemberHelpers<UserCreatedObject>.Assign(constraints, inputObjects, duplicateCount).ToList();
                    Assert.AreEqual(groups, returnedObjects.Count());
                    if(inputIds.Length * duplicateCount > returnedObjects.Sum(group => group.Members.Count()))
                    {
                        Assert.IsTrue(returnedObjects.All(group => group.Members.Count() == maxPerGroup));
                    }
                    else
                    {
                        Assert.AreEqual(inputIds.Length * duplicateCount, returnedObjects.Sum(group => group.Members.Count()));
                        Assert.IsTrue(returnedObjects.All(group => group.Members.Count() <= maxPerGroup));
                    }
                }
                catch (Exception e)
                {
                    exc = e;
                    failCount++;
                }
            }
            Assert.AreEqual(0, failCount, message: $"Success rate ({rerunCount - failCount}/{rerunCount}) not 100%. Example Exception: {exc}");
        }
    }
}
