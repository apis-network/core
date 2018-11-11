using System;
using System.Linq;
using System.Threading.Tasks;

using EnsureThat;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace ApisNetwork.Core.RateLimiter
{
    public class IpRateLimiter : IRateLimiter
    {
        private readonly IMemoryCache _memoryCache;

        private readonly TimeSpan _cacheDuration;

        private const string CloudflareIpHeaderKey = "CF-Connecting-IP";
        private const string XForwadedForHeaderKey = "X-Forwarded-For";


        /// <summary>
        /// Initializes a new instance of the ApisNetwork.Core.RateLimiter.IpRateLimiter class.
        /// </summary>
        /// <param name="memoryCache">The memory cache. This cannot be null.</param>
        /// <param name="cacheDuration">Duration of the cache.</param>
        public IpRateLimiter([NotNull] IMemoryCache memoryCache, TimeSpan cacheDuration)
        {
            EnsureArg.IsNotNull(memoryCache);
            EnsureArg.IsNotDefault(cacheDuration);

            this._memoryCache = memoryCache;
            this._cacheDuration = cacheDuration;
        }


        /// <summary>
        /// Determine if we can execute asynchronously the action.
        /// </summary>
        /// <param name="request">The current request. This cannot be null.</param>
        /// <returns>
        /// True if we can execute the action, false if not.
        /// </returns>
        public Task<bool> CanExecuteAsync(HttpRequest request)
        {
            EnsureArg.IsNotNull(request);

            string ip;

            if (request.Headers.TryGetValue(CloudflareIpHeaderKey, out var ips)
                || request.Headers.TryGetValue(XForwadedForHeaderKey, out ips))
            {
                ip = ips.First();
            }
            else
            {
                ip = request.HttpContext.Connection.RemoteIpAddress.ToString();
            }

            var canExecute = !this._memoryCache.TryGetValue(ip, out _);

            if (canExecute)
            {
                using (var entry = this._memoryCache.CreateEntry(ip))
                {
                    entry.SlidingExpiration = this._cacheDuration;
                }
            }

            return Task.FromResult(canExecute);
        }
    }
}
