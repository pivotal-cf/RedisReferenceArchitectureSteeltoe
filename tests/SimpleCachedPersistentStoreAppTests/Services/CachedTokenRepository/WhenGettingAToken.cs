using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using SimpleCachedPersistentStoreApp.Boundaries;
using SimpleCachedPersistentStoreApp.Services;
using Models = SimpleCachedPersistentStoreApp.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;

namespace SimpleCachedPersistentStoreAppTests.Services.CachedTokenRepository
{
    public class WhenGettingAToken
    {
        private IDistributedCache _cache;
        private ITokenDB _tokenDb;
        private ILogger<CachedTokenRepositoryService> _logger;
        private CachedTokenRepositoryService _sut;

        public WhenGettingAToken()
        {
            _cache = Substitute.For<IDistributedCache>();
            _tokenDb = Substitute.For<ITokenDB>();
            _logger = Substitute.For<ILogger<CachedTokenRepositoryService>>();
            _sut = new CachedTokenRepositoryService(_cache, _tokenDb, _logger);
        }

        [Theory]
        [InlineData("foo", "bar")]
        [InlineData("Bang", "Bazz")]
        public async Task ReturnsValuesThatExistInCache(string expectedKey, string expectedValue)
        {
            var expectedByteValue = Encoding.ASCII.GetBytes(expectedValue);
            _cache.GetAsync(expectedKey).Returns(expectedByteValue);

            var expectedToken = await _sut.GetToken(expectedKey);

            Assert.IsType<Models.Token>(expectedToken);
            Assert.Equal(expectedKey, expectedToken.Key);
            Assert.Equal(expectedValue, expectedToken.Value);
            await _cache.Received(1).GetAsync(expectedKey);
            await _tokenDb.DidNotReceiveWithAnyArgs().GetToken("");
        }

        [Theory]
        [InlineData("foo", "bar")]
        [InlineData("Bang", "Bazz")]
        public async Task ReturnsValuesThatExistInSQL_ButNotInCache(string expectedKey, string expectedValue)
        {
            _cache.GetAsync(expectedKey).Returns<byte[]>(x => null);
            _tokenDb.GetToken(expectedKey).Returns(new Models.Token(){
                 Key = expectedKey,
                 Value = expectedValue
            });

            var expectedToken = await _sut.GetToken(expectedKey);

            Assert.IsType<Models.Token>(expectedToken);
            Assert.Equal(expectedKey, expectedToken.Key);
            Assert.Equal(expectedValue, expectedToken.Value);
            await _cache.Received(1).GetAsync(expectedKey);
            await _tokenDb.Received(1).GetToken(expectedKey);
        }

        [Fact]
        public async Task ValuesThatOnlyExistInSQL_GetAddedToCache()
        {
            var expectedKey = "foo";
            var expectedValue = "bar";

            _cache.GetAsync(expectedKey).Returns<byte[]>(x => null);
            _tokenDb.GetToken(expectedKey).Returns(new Models.Token(){
                 Key = expectedKey,
                 Value = expectedValue
            });

            var expectedToken = await _sut.GetToken(expectedKey);

            await _cache.Received(1).GetAsync(expectedKey);
            await _tokenDb.Received(1).GetToken(expectedKey);
            await _cache.Received(1)
                    .SetAsync(
                        expectedKey, 
                        Arg.Is<byte[]>(x => x.SequenceEqual(Encoding.ASCII.GetBytes(expectedValue))), 
                        Arg.Any<DistributedCacheEntryOptions>(), 
                        Arg.Any<System.Threading.CancellationToken>()
                    );
        }

        [Fact]
        public async Task ReturnsNullForNonExistingKeyInBothStorages()
        {
            _cache.GetAsync("foo").Returns<byte[]>(x => null);
            _tokenDb.GetToken("foo").Returns<Models.Token>(x => null);

            var expectedToken = await _sut.GetToken("foo");

            Assert.Null(expectedToken);
            await _cache.Received(1).GetAsync("foo");
            await _tokenDb.Received(1).GetToken("foo");
            await _cache.DidNotReceiveWithAnyArgs().SetAsync("", new byte[1]);
        }

        [Fact]
        public async Task MalformedTokensFromSql_DoNotCauseErrors()
        {
            _cache.GetAsync("foo").Returns<byte[]>(x => null);
            _tokenDb.GetToken("foo").Returns<Models.Token>(new Models.Token());

            var expectedToken = await _sut.GetToken("foo");

            Assert.Null(expectedToken);
            await _cache.Received(1).GetAsync("foo");
            await _tokenDb.Received(1).GetToken("foo");
        }

        [Fact]
        public async Task MalformedTokensFromSql_DoNotGetAddedToCache()
        {
            _cache.GetAsync("foo").Returns<byte[]>(x => null);
            _tokenDb.GetToken("foo").Returns<Models.Token>(new Models.Token());

            var expectedToken = await _sut.GetToken("foo");

            await _cache.Received(1).GetAsync("foo");
            await _tokenDb.Received(1).GetToken("foo");
            await _cache.DidNotReceiveWithAnyArgs().SetAsync("", new byte[1]);
        }
    }
}