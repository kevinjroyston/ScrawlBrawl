using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public interface IAccessor<T>
    {
        public bool Refresh();
        public T Value { get; }
    }
}
