using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using ApisNetwork.Core.RateLimiter;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

using Moq;

using Xunit;

namespace ApisNetwork.Core.Tests.RateLimiter
{
    public class IpRateLimiterTests
    {
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMilliseconds(10);

        private readonly IpRateLimiter _ipRateLimiter;

        private readonly Mock<HttpRequest> _requestMock;

        public IpRateLimiterTests()
        {
            var ip = new StringValues("1.1.1.1");
            this._requestMock = new Mock<HttpRequest>();
            this._requestMock.Setup(request => request.Headers.TryGetValue(It.IsAny<string>(), out ip)).Returns(true);
            this._ipRateLimiter = new IpRateLimiter(
                new MemoryCache(new MemoryCacheOptions()),
                this._defaultCacheDuration);
        }

        [Fact]
        public async Task ShouldBlockRegisteredIps()
        {
            await this._ipRateLimiter.CanExecuteAsync(this._requestMock.Object);
            var canExecute = await this._ipRateLimiter.CanExecuteAsync(this._requestMock.Object);
            Assert.False(canExecute);
        }

        [Fact]
        public async Task ShouldNotBlockNewIps()
        {
            var canExecute = await this._ipRateLimiter.CanExecuteAsync(this._requestMock.Object);
            Assert.True(canExecute);
        }

        [Fact]
        public async Task ShouldNotBlockRegisteredIpsAfterCacheExpiration()
        {
            await this._ipRateLimiter.CanExecuteAsync(this._requestMock.Object);
            Thread.Sleep(this._defaultCacheDuration);
            var canExecute = await this._ipRateLimiter.CanExecuteAsync(this._requestMock.Object);
            Assert.True(canExecute);
        }

        [Fact]
        public async Task ShouldTakeRemoteIpWhenNoHeader()
        {
            var ip = new StringValues("1.1.1.1");
            var expectedIp = IPAddress.Parse("2.2.2.2");
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            this._requestMock.Setup(request => request.Headers.TryGetValue(It.IsAny<string>(), out ip)).Returns(false);
            this._requestMock.Setup(request => request.HttpContext.Connection.RemoteIpAddress).Returns(expectedIp);

            var rateLimiter = new IpRateLimiter(memoryCache, this._defaultCacheDuration);
            var canExecute = await rateLimiter.CanExecuteAsync(this._requestMock.Object);

            Assert.True(canExecute);
            Assert.True(memoryCache.TryGetValue(expectedIp.ToString(), out _));
        }
    }
}
