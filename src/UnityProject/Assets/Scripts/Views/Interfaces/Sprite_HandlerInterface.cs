using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Views.Interfaces
{
    public class SpriteHolder
    {
        public IReadOnlyList<Sprite> Sprites { get; set; }
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }
        public UnityField<IReadOnlyList<int>> BackgroundColor { get; set; }
    }
    public interface Sprite_HandlerInterface : HandlerInterface<SpriteHolder>
    {
        void UpdateValue(SpriteHolder value);
    }
}
