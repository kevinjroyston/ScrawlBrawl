using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Views.Interfaces
{
    public enum IntType
    {
        Object_VoteCount
    }
    public interface Ints_HandlerInterface
    {
        IntType Type { get; set; }
        void UpdateValue(UnityField<int?> value);
    }
}
