namespace Backend.GameInfrastructure.DataModels.Users
{
    public enum UserActivity
    {
        Active,
        Inactive,
        Disconnected
    }
    public enum UserStatus
    {
        AnsweringPrompts,
        Waiting,
    }
    public enum SubmitType
    {
        None,
        Manual,
        Auto
    }
}
