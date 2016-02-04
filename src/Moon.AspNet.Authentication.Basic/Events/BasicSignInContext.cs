using System.Security.Claims;
using Microsoft.AspNet.Http;

namespace Moon.AspNet.Authentication.Basic
{
    /// <summary>
    /// Context object used to control flow of Basic authentication.
    /// </summary>
    public class BasicSignInContext : BaseBasicContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <param name="options">The middleware options.</param>
        /// <param name="userName">The user name (login, e-mail, etc.) used.</param>
        /// <param name="password">The password (secret).</param>
        public BasicSignInContext(HttpContext context, BasicAuthenticationOptions options, string userName, string password)
            : base(context, options)
        {
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// Gets or sets the user name (login, e-mail, etc.) used.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password (secret).
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the principal that will be returned to the application.
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }
    }
}