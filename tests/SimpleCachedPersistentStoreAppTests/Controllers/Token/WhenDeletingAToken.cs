using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Microsoft.AspNetCore.Mvc;
using SimpleCachedPersistentStoreApp.Controllers;
using SimpleCachedPersistentStoreApp.Services;
using Microsoft.Extensions.Logging;

namespace SimpleCachedPersistentStoreAppTests.Controllers.Token
{
    public class WhenDeletingAToken
    {
        ITokenService _tokenRepository;
        TokenController _sut;
        ILogger<TokenController> _logger;

        public WhenDeletingAToken()
        {
            _tokenRepository = Substitute.For<ITokenService>();
            _logger = Substitute.For<ILogger<TokenController>>();
            _sut = new TokenController(_tokenRepository, _logger);
        }

        [Fact]
        public async Task CorrectTokenSuccessfullyDeleted()
        {
            var action = await _sut.DeleteKeyValue("foo");
            var result = (OkResult)action;

            Assert.Equal(200, result.StatusCode);
            Assert.IsType<OkResult>(result);

            await _tokenRepository.Received().DeleteToken("foo");
        }

        [Fact]
        public async Task HandlesArgumentExceptions()
        {
            _tokenRepository.DeleteToken("").ThrowsForAnyArgs(new ArgumentException("Key cannot be null or empty"));
            
            var action = await _sut.DeleteKeyValue("");
            var result = (BadRequestResult)action;

            Assert.Equal(400, result.StatusCode);
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
