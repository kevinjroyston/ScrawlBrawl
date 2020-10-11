namespace Backend.APIs.DataModels.UnityObjects
{
    public abstract class UnityObject : IAccessorHashable
    {
        public abstract int GetIAccessorHashCode();
        public abstract bool Refresh();
    }
}
