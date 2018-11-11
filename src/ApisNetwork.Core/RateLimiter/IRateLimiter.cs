using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;

namespace ApisNetwork.Core.RateLimiter
{
    /// <summary>
    /// Interface for rate limiter.
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Determine if we can execute asynchronously the action.
        /// </summary>
        /// <param name="request">The current request. This cannot be null.</param>
        /// <returns>
        /// True if we can execute the action, false if not.
        /// </returns>
        Task<bool> CanExecuteAsync([NotNull] HttpRequest request);
    }
}
