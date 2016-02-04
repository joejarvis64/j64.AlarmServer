using System;
using Moon;
using Moon.AspNet.Authentication.Basic;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// <see cref="IApplicationBuilder" /> extension methods.
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds a Basic authentication middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
            => app.UseBasicAuthentication(new BasicAuthenticationOptions());

        /// <summary>
        /// Adds a Basic authentication middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="configureOptions">The middleware options configuration.</param>
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, Action<BasicAuthenticationOptions> configureOptions)
        {
            var options = new BasicAuthenticationOptions();

            if (configureOptions != null)
            {
                configureOptions(options);
            }

            return app.UseBasicAuthentication(options);
        }

        /// <summary>
        /// Adds a cookie-based authentication middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="options">The middleware options configuration.</param>
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, BasicAuthenticationOptions options)
        {
            //Requires.NotNull(options, nameof(options));

            return app.UseMiddleware<BasicAuthenticationMiddleware>(options);
        }
    }
}