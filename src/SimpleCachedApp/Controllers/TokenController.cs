using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SimpleCachedApp.Models;

namespace SimpleCachedApp.Controllers
{
    [ApiController]
    [Route("/token")]
    public class TokenController : Controller
    {
        IDistributedCache _cache;
        ILogger<TokenController> _logger;

        public TokenController(IDistributedCache cache, ILogger<TokenController> logger)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpPost]
        [Route("{tokenKey}")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetKeyValue(string tokenKey, [FromQuery]string tokenValue)
        {
            byte[] bytes;
            try 
            {
                bytes = Encoding.ASCII.GetBytes(tokenValue);
            }
            catch(ArgumentNullException ae)
            {
                _logger.LogError(ae, "Failed to encode value to bytes");
                return BadRequest(ae);
            }

            try 
            {
                await _cache.SetAsync(tokenKey, bytes);
            }
            catch(ArgumentNullException ae)
            {
                _logger.LogError(ae, "Key or Value are not valid tokens, ensure you enter strings");
                return BadRequest(ae);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An Error has occured setting the token");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return CreatedAtAction(nameof(GetKeyValue), new { key = tokenKey }, new Token(){Key = tokenKey, Value = tokenValue});
        }

        [HttpGet]
        [Route("{tokenKey}")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetKeyValue(string tokenKey)
        {
            var token = new Token
            { 
                Key = tokenKey
            };

            var tokenByteValue = await _cache.GetAsync(tokenKey);
            if (tokenByteValue != null) 
            {
                token.Value = Encoding.ASCII.GetString(tokenByteValue);
                return Ok(token);
            }

            return NotFound();
        }

        [HttpDelete]
        [Route("{tokenKey}")]
        [Produces("application/json")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteKeyValue(string tokenKey)
        {
            await _cache.RemoveAsync(tokenKey);
            return Ok();
        }
    }
}
