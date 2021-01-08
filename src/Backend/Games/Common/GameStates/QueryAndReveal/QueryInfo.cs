using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.QueryAndReveal
{
    public class QueryInfo<T>
    {
        public User UserQueried { get; set; }
        public double TimeTakenInMs { get; set; }
        public T Answer { get; set; }
    }
}
