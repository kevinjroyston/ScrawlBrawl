using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;

public class UnityUserStatuses
{
    [JsonProperty("a")]
    public List<Guid> UsersAnsweringPrompts {get; set; }
}
