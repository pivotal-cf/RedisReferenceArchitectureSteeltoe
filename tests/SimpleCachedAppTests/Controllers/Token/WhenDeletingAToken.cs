using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using SimpleCachedApp.Controllers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace SimpleCachedAppTests.Controllers.Token
{
    public class WhenDeletingAToken
    {
        IDistributedCache _cache;
        TokenController _sut;
        ILogger<TokenController> _logger;

        public WhenDeletingAToken()
        {
            _cache = Substitute.For<IDistributedCache>();
            _logger = Substitute.For<ILogger<TokenController>>();
            _sut = new TokenController(_cache, _logger);
        }

        [Fact]
        public async Task TokenIsDeletedAndCorrectlyValidated()
        {
            var action = await _sut.DeleteKeyValue("foo");
            var result = (OkResult)action;

            Assert.Equal(200, result.StatusCode);
            Assert.IsType<OkResult>(result);

            await _cache.Received().RemoveAsync("foo");
        }
    }
}
