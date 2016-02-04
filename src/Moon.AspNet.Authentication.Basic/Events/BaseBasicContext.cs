using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;

namespace Moon.AspNet.Authentication.Basic
{
    /// <summary>
    /// The base class for event contexts.
    /// </summary>
    public class BaseBasicContext : BaseContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBasicContext" /> class.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <param name="options">The middleware options.</param>
        public BaseBasicContext(HttpContext context, BasicAuthenticationOptions options)
            : base(context)
        {
            //Requires.NotNull(options, nameof(options));

            Options = options;
        }

        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        public BasicAuthenticationOptions Options { get; }
    }
}