using System;
using System.Threading.Tasks;

namespace Moon.AspNet.Authentication.Basic
{
    /// <summary>
    /// This default implementation of the <see cref="IBasicAuthenticationEvents" /> may be used if
    /// the application only needs to override a few of the interface methods. This may be used as a
    /// base class or may be instantiated directly.
    /// </summary>
    public class BasicAuthenticationEvents : IBasicAuthenticationEvents
    {
        /// <summary>
        /// A delegate assigned to this property will be invoked when the related method is called.
        /// </summary>
        public Func<BasicSignInContext, Task> OnSignIn { get; set; } = ctx => Task.CompletedTask;

        /// <summary>
        /// Implements the interface method by invoking the related delegate method.
        /// </summary>
        /// <param name="context">Contains information about the event.</param>
        public virtual Task SignInAsync(BasicSignInContext context)
            => OnSignIn(context);
    }
}