using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using SimpleCachedPersistentStoreApp.Controllers;
using Models = SimpleCachedPersistentStoreApp.Models;
using SimpleCachedPersistentStoreApp.Services;
using Microsoft.Extensions.Logging;

namespace SimpleCachedPersistentStoreAppTests.Controllers.Token
{
    public class WhenGettingAToken
    {
        ITokenService _tokenRepository;
        TokenController _sut;
        ILogger<TokenController> _logger;

        public WhenGettingAToken()
        {
            _tokenRepository = Substitute.For<ITokenService>();
            _logger = Substitute.For<ILogger<TokenController>>();
            _sut = new TokenController(_tokenRepository, _logger);
        }

        [Fact]
        public async Task WhenTokenExists_TokenIsReturnedCorrectly()
        {
            var expectedToken = new Models.Token(){Key = "Foo", Value = "Bar"};

            _tokenRepository.GetToken("Foo").Returns(
                                Task.FromResult<Models.Token>(expectedToken));

            var action = await _sut.GetKeyValue("Foo");
            var result = (OkObjectResult)action;
            var returnToken = (Models.Token)result.Value;

            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Foo", returnToken.Key);
            Assert.Equal("Bar", returnToken.Value);

            await _tokenRepository.Received().GetToken("Foo");
        }

        [Fact]
        public async Task WhenTokenDoesNotExist_CorrectStatusCodeIsReturned()
        {
            _tokenRepository.GetToken("buzz").Returns(Task.FromResult<Models.Token>(null));

            var action = await _sut.GetKeyValue("buzz");
            var result = (NotFoundResult)action;

            Assert.Equal(404, result.StatusCode);
            Assert.IsType<NotFoundResult>(result);

            await _tokenRepository.Received().GetToken("buzz");
        }
    }
}
