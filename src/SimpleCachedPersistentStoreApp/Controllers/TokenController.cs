using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using SimpleCachedPersistentStoreApp.Models;
using SimpleCachedPersistentStoreApp.Services;

namespace SimpleCachedPersistentStoreApp.Controllers
{
    [ApiController]
    [Route("/token")]
    public class TokenController : Controller
    {
        ITokenService _tokenRepository;
        ILogger<TokenController> _logger;

        public TokenController(ITokenService tokenRepository, ILogger<TokenController> logger)
        {
            _logger = logger;
            _tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("{tokenKey}")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Creates A Token On The Storage and replicates on cache", typeof(Token))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid set of inputs provided")]
        public async Task<IActionResult> SetKeyValue(string tokenKey, [FromQuery]string tokenValue)
        {
            if (string.IsNullOrEmpty(tokenValue) || string.IsNullOrEmpty(tokenKey))
            {
                var errorMessage = "Token Key or Value cannot be null";
                _logger.LogError($"{errorMessage} {{RequestDetails}}", new
                {
                    Key = tokenKey,
                    Value = tokenValue,
                    ExceptionMessage = "Token Key or Value cannot be null"
                });

                return BadRequest(errorMessage);
            }

            try 
            {
                await _tokenRepository.InsertToken(tokenKey, tokenValue);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error has occurred setting key value pair {RequestDetails}", new
                {
                    Key = tokenKey,
                    Value = tokenValue,
                    ExceptionMessage = ex.Message
                });

                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            // This log is here for example sake. Do not send plaintext values to Logs
            _logger.LogInformation($"Created Token: {tokenKey}");

            return CreatedAtAction(nameof(GetKeyValue), new { key = tokenKey }, new Token {Key = tokenKey, Value = tokenValue});
        }

        [HttpGet]
        [Route("{tokenKey}")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Retrieves a token from cache or storage", typeof(Token))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Token Key not found in cache or storage")]
        public async Task<IActionResult> GetKeyValue(string tokenKey)
        {
            var tokenResult = await _tokenRepository.GetToken(tokenKey);
            if (tokenResult != null) 
            {
                return Ok(tokenResult);
            }

            return NotFound();
        }

        [HttpDelete]
        [Route("{tokenKey}")]
        [Produces("application/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Deleted a token from cache or storage")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Token Key not found in cache or storage")]
        public async Task<IActionResult> DeleteKeyValue(string tokenKey)
        {
            try 
            {
                await _tokenRepository.DeleteToken(tokenKey);
                return Ok();
            }
            catch (ArgumentException ae)
            {
                 _logger.LogError(ae, "When deleting tokens, key cannot be null or empty {RequestDetails}", new
                {
                    Key = tokenKey,
                    ExceptionMessage = ae.Message
                });

                return BadRequest();
            }
        }
    }
}
