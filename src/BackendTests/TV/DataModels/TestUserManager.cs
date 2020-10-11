using Backend.GameInfrastructure.DataModels.Users;
using System.Collections.Generic;

namespace BackendTests.TV.DataModels
{
    static class TestUserManager
    {
        private static Dictionary<int, User> IdToTestUser = new Dictionary<int, User>();
        
        public static User GetTestUser(int id)
        {
            if (!IdToTestUser.ContainsKey(id))
            {
                IdToTestUser.Add(id, new User("TestUser" + id) { DisplayName = $"{id}" });
            }
            return IdToTestUser[id];
        }

        public static void ResetTestUsers()
        {
            IdToTestUser = new Dictionary<int, User>();
        }
    }
}
