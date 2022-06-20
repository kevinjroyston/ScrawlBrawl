﻿using Backend.GameInfrastructure.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityImage : UnityObject
    {
        public IReadOnlyList<string> Base64Pngs => DrawingObjects.Select(drawing => drawing.Id.ToString()).ToList();

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyList<DrawingObject> DrawingObjects {get; set;}
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }

        public UnityImage() : base()
        {
            this.Type = UnityObjectType.Image;
        }
        public UnityImage(Guid UnityObjectId) : base(UnityObjectId)
        {
            this.Type = UnityObjectType.Image;
        }
        public UnityImage(UnityObject other) : base(other)
        {
            this.Type = UnityObjectType.Image;
        }
    }
}
