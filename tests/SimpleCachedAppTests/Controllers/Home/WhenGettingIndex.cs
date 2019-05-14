using Microsoft.AspNetCore.Mvc;
using SimpleCachedApp.Controllers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using System.Threading.Tasks;

namespace SimpleCachedAppTests.Controllers.Home
{
    public class WhenGettingIndex
    {
        HomeController _sut;
        ILogger<HomeController> _logger;

        public WhenGettingIndex()
        {
            _logger = Substitute.For<ILogger<HomeController>>();
            _sut = new HomeController(_logger);
        }

        [Fact]
        public async Task WeAreRedirectedToSwaggerPage()
        {
            var result = await _sut.Index();
            var action = (RedirectResult)result;

            Assert.Contains("~/swagger", action.Url);
        }
    }
}