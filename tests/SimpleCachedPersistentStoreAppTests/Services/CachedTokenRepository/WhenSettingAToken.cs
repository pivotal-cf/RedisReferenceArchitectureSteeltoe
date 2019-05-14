using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SimpleCachedPersistentStoreApp.Boundaries;
using SimpleCachedPersistentStoreApp.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;

namespace SimpleCachedPersistentStoreAppTests.Services.CachedTokenRepository
{
    public class WhenSettingAToken
    {
        private readonly IDistributedCache _cache;
        private readonly ITokenDB _tokenDb;
        private ILogger<CachedTokenRepositoryService> _logger;
        private CachedTokenRepositoryService _sut;

        public WhenSettingAToken()
        {
            _cache = Substitute.For<IDistributedCache>();
            _tokenDb = Substitute.For<ITokenDB>();
            _logger = Substitute.For<ILogger<CachedTokenRepositoryService>>();
            _sut = new CachedTokenRepositoryService(_cache, _tokenDb, _logger);
        }

        [Fact]
        public async Task EnsureTokenIsWrittenToBothCacheAndDB()
        {
            var valueAsBytes = Encoding.ASCII.GetBytes("bar");

            await _sut.InsertToken("foo", "bar");

            await _tokenDb.Received(1).InsertToken("foo", "bar");
            await _cache.Received(1)
                    .SetAsync(
                        "foo", 
                        Arg.Is<byte[]>(x => valueAsBytes.SequenceEqual(x)), 
                        Arg.Any<DistributedCacheEntryOptions>(), 
                        Arg.Any<System.Threading.CancellationToken>()
                    );
        }


        [Fact]
        public async Task DoesNotSwallowTokenDbExceptions()
        {
            _tokenDb.InsertToken("foo", "bar").ThrowsForAnyArgs(new Exception());

            await Assert.ThrowsAsync<Exception>(() => _sut.InsertToken("foo", "bar"));
        }

        [Fact]
        public async Task DoesNotBlowUpOnRedisExceptions()
        {
            _cache.SetAsync("foo", new byte[1]).ThrowsForAnyArgs(new ArgumentNullException());

            await _sut.InsertToken("foo", "bar");
        }

        [Theory]
        [InlineData("", "bar")]
        [InlineData(null, "bar")]
        [InlineData("foo", "")]
        [InlineData("foo", null)]
        public async Task InvalidInputsAreHandledCorrectly(string expectedKey, string expectedValue)
        {
            var expectedException = await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.InsertToken(expectedKey, expectedValue));
            Assert.Contains("null or empty", expectedException.Message);

            await _cache.DidNotReceiveWithAnyArgs().RemoveAsync("foo");
            await _tokenDb.DidNotReceiveWithAnyArgs().DeleteToken("foo");
        }    
    }
}