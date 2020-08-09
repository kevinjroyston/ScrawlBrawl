using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public abstract class UnityObject : IAccessorHashable
    {
        public abstract int GetIAccessorHashCode();
        public abstract bool Refresh();
    }
}
