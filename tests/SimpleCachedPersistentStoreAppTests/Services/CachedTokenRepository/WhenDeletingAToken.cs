using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using SimpleCachedPersistentStoreApp.Boundaries;
using SimpleCachedPersistentStoreApp.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace SimpleCachedPersistentStoreAppTests.Services.CachedTokenRepository
{
    public class WhenDeletingAToken
    {
        private IDistributedCache _cache;
        private ITokenDB _tokenDb;
        private ILogger<CachedTokenRepositoryService> _logger;
        private CachedTokenRepositoryService _sut;

        public WhenDeletingAToken()
        {
            _cache = Substitute.For<IDistributedCache>();
            _tokenDb = Substitute.For<ITokenDB>();
            _logger = Substitute.For<ILogger<CachedTokenRepositoryService>>();
            _sut = new CachedTokenRepositoryService(_cache, _tokenDb, _logger);
        }

        [Fact]
        public async Task CallsBothDatastoresCorrectly()
        {
            await _sut.DeleteToken("foo");

            await _cache.Received(1).RemoveAsync("foo");
            await _tokenDb.Received(1).DeleteToken("foo");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task InvalidKeysAreHandled(string keyValue)
        {
            var expectedException = await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.DeleteToken(keyValue));
            Assert.Contains("null or empty", expectedException.Message);

            await _cache.DidNotReceiveWithAnyArgs().RemoveAsync("foo");
            await _tokenDb.DidNotReceiveWithAnyArgs().DeleteToken("foo");
        }
    }
}