using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnityImage
{
    public IReadOnlyList<string> _Base64Pngs { private get; set; }
    public IReadOnlyList<Sprite> PngSprites
    {
        get
        {
            return InternalPngSprites ?? (InternalPngSprites = _Base64Pngs?.Select(val => TextureUtilities.LoadTextureFromBase64(val)).ToList());
        }
    }
    private List<Sprite> InternalPngSprites = null;


    public int? _SpriteGridWidth { get; set; } = 1;
    public int? _SpriteGridHeight { get; set; } = 1;
    public string _Title { get; set; }
    public string _Header { get; set; }
    public string _Footer { get; set; }
    public string _ImageIdentifier { get; set; }
    public int? _VoteCount { get; set; }
    //public IReadOnlyList<User> _RelevantUsers { get; set; }
    public IReadOnlyList<int> _BackgroundColor { get; set; }
}