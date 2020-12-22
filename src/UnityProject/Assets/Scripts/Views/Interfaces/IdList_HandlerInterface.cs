using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
    public enum IdType
    {
        Object_UsersWhoVotedFor
    }

    public interface IdList_HandlerInterface : HandlerInterface<IReadOnlyList<Guid>>
    {
        IdType Type { get; set; }
        void UpdateValue(IReadOnlyList<Guid> list);
    }
}
