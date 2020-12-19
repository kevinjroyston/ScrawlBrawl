using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{
    public class Legacy_UnityImageVoteRevealOptions
    {
        public IReadOnlyList<Legacy_User> _RelevantUsers { get; set; }
        public bool? _RevealThisImage { get; set; }
    }
}
