using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityRoundDetails
{
    [JsonProperty("a")]
    public int CurrentRound { get; set; }

    [JsonProperty("b")]
    public int MaxRounds { get; set; }
}
