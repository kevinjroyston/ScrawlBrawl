using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityView
{
    public Guid Id { get; } = Guid.NewGuid();
    //public UnityViewOptions Options { get; set; }
    public IReadOnlyList<UnityImage> _UnityImages { get; set; }
    public TVScreenId _ScreenId { get; set; }
    //public IReadOnlyList<User> _Users { get; set; }
    public string _Title { get; set; }
    public string _Instructions { get; set; }
}
