using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
    public interface UsersList_HandlerInterface
    {
        void UpdateValue(IReadOnlyList<UnityUser> list);
    }
}
