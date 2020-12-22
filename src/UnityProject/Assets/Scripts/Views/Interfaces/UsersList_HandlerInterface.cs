using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
    public class UsersListHolder
    {
        public IReadOnlyList<UnityUser> Users { get; set; }
        public bool IsRevealing { get; set; } = false;
    }
    public interface UsersList_HandlerInterface : HandlerInterface<UsersListHolder>
    {
        void UpdateValue(UsersListHolder users);
    }
}
