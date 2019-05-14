using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using SimpleCachedApp.Controllers;
using Models = SimpleCachedApp.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SimpleCachedAppTests.Controllers.Token
{
    public class WhenGettingAToken
    {
        IDistributedCache _cache;
        TokenController _sut;
        ILogger<TokenController> _logger;

        public WhenGettingAToken()
        {
            _cache = Substitute.For<IDistributedCache>();
            _logger = Substitute.For<ILogger<TokenController>>();
            _sut = new TokenController(_cache, _logger);
        }

        [Fact]
        public async Task WhenTokenExists_TokenIsReturnedCorrectly()
        {
            _cache.GetAsync("foo").Returns(Encoding.ASCII.GetBytes("bar"));

            var action = await _sut.GetKeyValue("foo");
            var result = (OkObjectResult)action;
            var returnToken = (Models.Token)result.Value;

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("foo", returnToken.Key);
            Assert.Equal("bar", returnToken.Value);

            await _cache.Received().GetAsync("foo");
        }

        [Fact]
        public async Task WhenTokenDoesNotExist_CorrectStatusCodeIsReturned()
        {
            _cache.GetAsync("foo").Returns((byte[])null);

            var action = await _sut.GetKeyValue("foo");
            var result = (NotFoundResult)action;

            Assert.Equal(404, result.StatusCode);
            Assert.IsType<NotFoundResult>(result);

            await _cache.Received().GetAsync("foo");
        }
    }
}
