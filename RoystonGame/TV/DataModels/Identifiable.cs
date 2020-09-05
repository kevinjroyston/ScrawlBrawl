using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.DataModels
{
    public interface Identifiable
    {
        Guid Id { get; }
    }
}
