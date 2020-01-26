using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.DiskData
{
    /// <summary>
    /// This will be stored to disk, accessible only by api for editing only by admins. The data will be used to control the service.
    /// </summary>
    public class ConfigurationEntry
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
