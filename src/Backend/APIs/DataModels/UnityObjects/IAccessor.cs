namespace Backend.APIs.DataModels.UnityObjects
{
    public interface IAccessor<T>
    {
        public bool Refresh();
        public T Value { get; }
    }
}
