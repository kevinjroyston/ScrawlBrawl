﻿using System;
using System.Text.Json.Serialization;
using KellermanSoftware.CompareNetObjects;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class DynamicAccessor<T> : IAccessor<T>
    {
        [JsonIgnore]
        public Func<T> DynamicBacker { get; set; }

        /// <summary>
        /// Refreshes the backed value. Returning true if it was changed.
        /// </summary>
        /// <returns>True if the backed value changed.</returns>
        public bool Refresh()
        {
            if(DynamicBacker == null)
            {
                return false;
            }

            T value = this.DynamicBacker.Invoke();
            CompareLogic compareLogic = new CompareLogic();
            ComparisonResult result = compareLogic.Compare(value, Value);
            Value = value;
            return !result.AreEqual;
        }

        public T Value { get; private set; }


    }
}