using System;

namespace RoystonGame.Web.Helpers.Validation
{
    public static class Arg
    {
        public static void NotNull<T>(T param, string paramName = null) where T : class
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
