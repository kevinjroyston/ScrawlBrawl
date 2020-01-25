using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Indicates this User is the party leader (Technically can have multiple).
    /// </summary>
    public bool IsPartyLeader { get; set; }

    /// <summary>
    /// The name to display for the user.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The self portrait of the user.
    /// </summary>
    public string SelfPortrait { get; set; }
    // TODO, sprite create cost is eaten by caller on main thread. can we get this onto a background thread?
    public Sprite SelfPortraitSprite {
        get
        {
            return InternalSelfPortraitSprite ?? (InternalSelfPortraitSprite = TextureUtilities.LoadTextureFromBase64(SelfPortrait));
        }
    }
    private Sprite InternalSelfPortraitSprite = null;

    public int Score { get; set; }
}
