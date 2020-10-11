using System;
using System.Collections.Generic;

namespace Backend.APIs.DataModels.UnityObjects
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
            // Default comparison
            bool toReturn = value?.Equals(Value) ?? Value == null;

            // IAccessor specific hashcode.
            IAccessorHashable hashableVal = value as IAccessorHashable;
            // IEnumerable of IAccessorHashable
            IEnumerable<IAccessorHashable> listHashableVal = value as IEnumerable<IAccessorHashable>;

            // TODO: IEnumerable of non IAccessorHashables (for drawing list and colors)
            // TODO: factor in the list order to the hash (multiply by index)
            int hashCode = 0;

            if (listHashableVal != null)
            {
                hashCode = 0;
                foreach (IAccessorHashable val in listHashableVal)
                {
                    hashCode ^= val.GetIAccessorHashCode();
                }
                toReturn = PriorHashCode != hashCode;
            }
            else if (hashableVal != null)
            {
                hashCode = hashableVal.GetIAccessorHashCode();
                toReturn = PriorHashCode != hashCode;
            }

            Value = value;
            PriorHashCode = hashCode;

            return toReturn;
        }

        private int PriorHashCode { get; set; } = 0;
        public T Value { get; private set; }


    }
}
