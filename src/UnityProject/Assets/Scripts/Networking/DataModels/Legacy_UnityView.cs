using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legacy_UnityView
{
    public Guid _Id { get; set; }
    //public UnityViewOptions Options { get; set; }
    public IReadOnlyList<Legacy_UnityImage> _UnityImages { get; set; }
    public TVScreenId _ScreenId { get; set; }
    public IReadOnlyList<Legacy_User> _Users { get; set; }
    public IReadOnlyList<Legacy_User> _VoteRevealUsers { get; set; }
    public Dictionary<string, int> _UserIdToDeltaScores { get; set; }
    public string _Title { get; set; }
    public string _Instructions { get; set; }
    public DateTime ServerTime { get; set; }
    public DateTime? _StateEndTime { get; set; }
    public Legacy_UnityViewOptions _Options { get; set; }
}
