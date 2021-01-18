using PostSharp.Patterns.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    [NotifyPropertyChanged]
    public class UnityImage : UnityObject
    {
        public IReadOnlyList<string> Base64Pngs { get; set; }
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }

        public UnityImage(Legacy_UnityImage legacy) : base(legacy)
        {
            this.Type = UnityObjectType.Image;
            this.Base64Pngs = legacy._Base64Pngs;
            this.SpriteGridWidth = legacy._SpriteGridWidth;
            this.SpriteGridHeight = legacy._SpriteGridHeight;
        }

        public UnityImage() : base()
        {
            this.Type = UnityObjectType.Image;
        }
        public UnityImage(Guid UnityObjectId) : base(UnityObjectId)
        {
            this.Type = UnityObjectType.Image;
        }
    }
}
