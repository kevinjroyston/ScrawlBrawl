namespace RoystonGame.Web.DataModels.UnityObjects
{
    public interface IAccessor<T>
    {
        public bool Refresh();
        public T Value { get; }
    }
}
