namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class StaticAccessor<T> : IAccessor<T>
    {
        public bool Refresh()
        {
            return false;
        }

        public T Value { get; set; }
    }
}
