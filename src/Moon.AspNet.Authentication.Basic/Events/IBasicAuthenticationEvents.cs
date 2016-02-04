using System.Threading.Tasks;

namespace Moon.AspNet.Authentication.Basic
{
    /// <summary>
    /// Specifies callback methods which the <see cref="BasicAuthenticationMiddleware" /> invokes to
    /// enable developer control over the authentication process.
    /// </summary>
    public interface IBasicAuthenticationEvents
    {
        /// <summary>
        /// Called when a request came with Basic authentication credentials. By implementing this
        /// method the credentials can be converted to a principal.
        /// </summary>
        /// <param name="context">Contains information about the sign-in request.</param>
        Task SignInAsync(BasicSignInContext context);
    }
}