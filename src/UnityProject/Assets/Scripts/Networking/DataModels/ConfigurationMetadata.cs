using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ConfigurationMetadata
{
    [JsonProperty("a")]
    public GameModeId? GameMode { get; set; }
}
