using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoystonGameTests.TV.DataModels
{
    static class TestUserManager
    {
        private static Dictionary<int, User> IdToTestUser = new Dictionary<int, User>();
        
        public static User GetTestUser(int id)
        {
            if (!IdToTestUser.ContainsKey(id))
            {
                IdToTestUser.Add(id, new User("TestUser" + id));
            }
            return IdToTestUser[id];
        }

        public static void ResetTestUsers()
        {
            IdToTestUser = new Dictionary<int, User>();
        }
    }
}
