namespace Moon.AspNet.Authentication.Basic
{
    /// <summary>
    /// Default values used by <see cref="BasicAuthenticationMiddleware" /> when not defined in <see cref="BasicAuthenticationOptions" />.
    /// </summary>
    public static class BasicAuthenticationDefaults
    {
        /// <summary>
        /// The default authentication scheme used by basic authentication.
        /// </summary>
        public const string AuthenticationScheme = "Basic";

        /// <summary>
        /// The default realm used by basic authentication.
        /// </summary>
        public const string Realm = "ASP.NET Core application";
    }
}