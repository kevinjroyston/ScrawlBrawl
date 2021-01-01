using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Networking.DataModels.UnityObjects
{
    public class UnityImage : UnityObject
    {
        public IReadOnlyList<string> Base64Pngs { private get; set; }
        public IReadOnlyList<Sprite> Sprites
        {
            get
            {
                return InternalPngSprites ?? (InternalPngSprites = Base64Pngs?.Where(val => val != null).Select(val => TextureUtilities.LoadTextureFromBase64(val)).ToList());
            }
        }
        private List<Sprite> InternalPngSprites = null;
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }

    }
}
