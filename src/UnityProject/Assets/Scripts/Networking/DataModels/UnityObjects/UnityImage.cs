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
        public IReadOnlyList<Sprite> Sprites { get; set; }
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }

    }
}
