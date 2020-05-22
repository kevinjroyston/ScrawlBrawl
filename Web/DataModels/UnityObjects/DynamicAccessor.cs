using KellermanSoftware.CompareNetObjects;
using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Generic;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class DynamicAccessor<T> : IAccessor<T>
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Func<T> DynamicBacker { get; set; }

        /// <summary>
        /// Refreshes the backed value. Returning true if it was changed.
        /// </summary>
        /// <returns>True if the backed value changed.</returns>
        public bool Refresh()
        {
            if (DynamicBacker == null)
            {
                return false;
            }

            T value = this.DynamicBacker.Invoke();
            CompareLogic compareLogic = new CompareLogic();
            ComparisonResult result = compareLogic.Compare(value, Value);
            // NEED TO DO A COPY OF VALUE / compare hashes
            Value = value;

            // TODO remove hack: Hacky user status fix
            bool toReturn = !result.AreEqual;
            IReadOnlyList<User> userVal = value as IReadOnlyList<User>;
            if (userVal != null)
            {
                foreach(User user in userVal)
                {
                    toReturn |= user.Dirty;
                    user.Dirty = false;
                }
            }

            return toReturn;
        }
        public T Value { get; private set; }


    }
}
