using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TypeEnums
{
    public enum HandlerScope
    {
        UnityObject,
        View
    }
    public enum HandlerType
    {
        IdList,
        Ints,
        ViewOptions,
        ObjectOptions,
        Sprite,
        Strings,
        Timer,
        UnityObjectList,
        UsersList,
        Color,
        SliderData,
        UnityViewHandler, // Sigh hacky fix to let object scope scripts inherit view handler ids. Long story.
        RoundDetails,
    }

    public class HandlerId
    {
        public HandlerType HandlerType { get; }
        public object SubType { get; }

        public HandlerId(HandlerType handlerType, object subType = null)
        {
            this.HandlerType = handlerType;
            this.SubType = subType;
        }

        public override int GetHashCode()
        {
            return HandlerType.GetHashCode()^SubType.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as HandlerId);
        }

        public bool Equals(HandlerId obj)
        {
            return obj != null && (this.HandlerType == obj.HandlerType && this.SubType == obj.SubType);
        }
    }

    [Serializable]
    public enum IdType
    {
        Object_UsersWhoVotedFor,
        Object_OwnerIds,
    }
    [Serializable]
    public enum IntType
    {
        Object_VoteCount
    }
    [Serializable]
    public enum StringType
    {
        View_Title,
        View_Instructions,
        Object_Title,
        Object_Header,
        Object_Footer,
        Object_ImageIdentifier,
    }

    [Serializable]
    public enum SliderType
    {
        MainSliderValue,
        GuessSliderValues,
    }
    public class TimerHolder
    {
        public DateTime? ServerTime { get; set; }
        public DateTime? StateEndTime { get; set; }
    }
    public class UsersListHolder
    {
        public List<UnityUser> Users { get; set; }
        public bool IsRevealing { get; set; } = false;

        public bool IsViewLoad { get; set; } = false;
    }
    public class SpriteHolder
    {
        public List<Sprite> Sprites { get; set; }
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }
        public UnityField<List<int>> BackgroundColor { get; set; }
    }
}
