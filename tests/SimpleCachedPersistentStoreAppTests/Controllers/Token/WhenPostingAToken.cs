using System;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Mvc;
using SimpleCachedPersistentStoreApp.Controllers;
using SimpleCachedPersistentStoreApp.Services;
using Models = SimpleCachedPersistentStoreApp.Models;
using Microsoft.Extensions.Logging;

namespace SimpleCachedPersistentStoreAppTests.Controllers.Token
{
    public class WhenPostingAToken
    {
        ITokenService _tokenRepository;
        TokenController _sut;
        ILogger<TokenController> _logger;

        public WhenPostingAToken()
        {
            _tokenRepository = Substitute.For<ITokenService>();
            _logger = Substitute.For<ILogger<TokenController>>();
            _sut = new TokenController(_tokenRepository, _logger);
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

            await _tokenRepository.Received().InsertToken("foo", "bar");
        }

        [Theory]
        [InlineData("foo", null)]
        [InlineData(null, "bar")]
        [InlineData("", null)]
        [InlineData(null, "")]
        public async Task ExceptionsInCacheClientAreHandledProperly(string key, string keyValue)
        {
            var action = await _sut.SetKeyValue(key, keyValue);
            var result = (BadRequestObjectResult)action;

            Assert.Equal(400, result.StatusCode);
            await _tokenRepository.DidNotReceiveWithAnyArgs().InsertToken("", "");
        }

        [Fact]
        public async Task OtherUnexpectedErrorsAreHandledProperly()
        {
            _tokenRepository.InsertToken("", "")
                .ReturnsForAnyArgs(x => 
                    throw new Exception("Disaster!")
                );

            var action = await _sut.SetKeyValue("foo", "bar");
            var result = (StatusCodeResult)action;

            Assert.Equal(500, result.StatusCode);
        }
    }
}
