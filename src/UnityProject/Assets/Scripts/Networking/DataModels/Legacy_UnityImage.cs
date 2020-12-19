using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System;
using Assets.Scripts.Networking.DataModels;

public class Legacy_UnityImage
{
    public IReadOnlyList<string> _Base64Pngs { private get; set; }
    public IReadOnlyList<Sprite> PngSprites
    {
        get
        {
            return InternalPngSprites ?? (InternalPngSprites = _Base64Pngs?.Where(val=>val!=null).Select(val => TextureUtilities.LoadTextureFromBase64(val)).ToList());
        }
    }
    private List<Sprite> InternalPngSprites = null;


    public int? _SpriteGridWidth { get; set; } = 1;
    public int? _SpriteGridHeight { get; set; } = 1;
    public string _Title { get; set; }
    public string _Header { get; set; }
    public string _Footer { get; set; }
    public string _ImageIdentifier { get; set; }
    public Guid? _ImageOwnerId { get; set; }
    public int? _VoteCount { get; set; }
    public Legacy_UnityImageVoteRevealOptions _VoteRevealOptions { get; set; }
    public IReadOnlyList<int> _BackgroundColor { get; set; }

    [JsonIgnore]
    public Legacy_UnityViewOptions Options { get; set; }
    public Guid _UnityImageId { get; set; }
}