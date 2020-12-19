using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legacy_User
{
    public Guid Id { get; set; }

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
    public int ScoreDeltaReveal { get; set; }
    public int ScoreDeltaScoreboard { get; set; }

    public UserActivity Activity { get; set; }
 
    public UserStatus Status { get; set; }


}
