using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using SimpleCachedApp.Controllers;
using Models = SimpleCachedApp.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace SimpleCachedAppTests.Controllers.Token
{
    public class WhenPostingAToken
    {
        IDistributedCache _cache;
        TokenController _sut;
        ILogger<TokenController> _logger;

        public WhenPostingAToken()
        {
            _cache = Substitute.For<IDistributedCache>();
            _logger = Substitute.For<ILogger<TokenController>>();
            _sut = new TokenController(_cache, _logger);
        }

        [Fact]
        public async Task ValueIsAddedAndCorrectlyValidated()
        {
            var action = await _sut.SetKeyValue("foo", "bar");
            var result = (CreatedAtActionResult)action;
            var returnToken = (Models.Token)result.Value;

            Assert.Equal(201, result.StatusCode);
            Assert.Equal("foo", returnToken.Key);
            Assert.Equal("bar", returnToken.Value);

            await _cache.Received().SetAsync(
                "foo", 
                Arg.Any<Byte[]>(), 
                Arg.Any<DistributedCacheEntryOptions>(), 
                Arg.Any<System.Threading.CancellationToken>()
            );
        }

        [Theory]
        [InlineData("foo", null)]
        [InlineData(null, "bar")]
        [InlineData("", null)]
        [InlineData(null, "")]
        public async Task ExceptionsInCacheClientAreHandledProperly(string key, string keyValue)
        {
            _cache.SetAsync("", new byte[0])
                .ReturnsForAnyArgs(x => 
                    throw new ArgumentNullException("Received a null argument")
                );

            var action = await _sut.SetKeyValue(key, keyValue);
            var result = (BadRequestObjectResult)action;

            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task OtherUnexpectedErrorsAreHandledProperly()
        {
            _cache.SetAsync("", new byte[0])
                .ReturnsForAnyArgs(x => 
                    throw new Exception("Disaster!")
                );

            var action = await _sut.SetKeyValue("foo", "bar");
            var result = (StatusCodeResult)action;

            Assert.Equal(500, result.StatusCode);
        }
    }
}
