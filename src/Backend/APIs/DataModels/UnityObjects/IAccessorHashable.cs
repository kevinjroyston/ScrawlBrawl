namespace Backend.APIs.DataModels.UnityObjects
{
    /// <summary>
    /// If a class implements this it will be called when determining if the object has changed.
    /// </summary>
    public interface IAccessorHashable
    {
        public int GetIAccessorHashCode();
    }
}
