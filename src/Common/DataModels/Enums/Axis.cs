namespace Common.DataModels.Enums
{
    public enum Axis
    {
        Horizontal = 0,  // This has not been tested for text. It is only used for images
        Vertical = 1    // I had to hack this together on the frontend. Vertical axis is completely hardcoded and breaks with more than 8 elements. Does not respect aspect ratio, so dont use this for images
    }
}
