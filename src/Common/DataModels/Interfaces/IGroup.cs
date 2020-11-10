using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Interfaces
{
    public interface IGroup<T>
    {
        public IEnumerable<T> Members { get; }
    }
}
