using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.DataModels.Requests
{
    public class UserSubForm
    {
        public Guid Id { get; set; }

        public string ShortAnswer { get; set; }

        public string Drawing { get; set; }

        public int? RadioAnswer { get; set; }
    }
}
