using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SimpleCachedPersistentStoreApp.Boundaries;
using SimpleCachedPersistentStoreApp.Models;

namespace SimpleCachedPersistentStoreApp.Services
{
    public class CachedTokenRepositoryService : ITokenService
    {
        private readonly IDistributedCache _cache;
        private readonly ITokenDB _tokenDb;
        private readonly ILogger<CachedTokenRepositoryService> _logger;

        public CachedTokenRepositoryService(IDistributedCache cache, ITokenDB tokenDb, ILogger<CachedTokenRepositoryService> logger)
        {
            _logger = logger;
            _cache = cache;
            _tokenDb = tokenDb;
        }

        public async Task DeleteToken(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                var errorMessage = "Key cannot be null or empty";

                _logger.LogWarning("Attempted to delete a token, but no key was provided");

                throw new ArgumentException(errorMessage);
            }

            await _tokenDb.DeleteToken(key);
            await _cache.RemoveAsync(key);

            // This log is here for example sake. Do not send plaintext values to Logs
            _logger.LogInformation($"Deleted Token {key}");
            return;
        }

        public async Task<Token> GetToken(string key)
        {
            var retrievedToken = await GetTokenFromCache(key);
            if (retrievedToken != null)
            {
                return retrievedToken;
            }

            retrievedToken = await GetTokenFromDb(key);
            if (retrievedToken != null)
            {
                await InsertTokenToCache(key, retrievedToken.Value);
                return retrievedToken;
            }

            return null;
        }

        public async Task InsertToken(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                var errorMessage = $"Neither Key or Value can be null or empty: key = {key} and value = {value}";

                _logger.LogWarning(errorMessage);

                throw new ArgumentException(errorMessage);
            }

            await _tokenDb.InsertToken(key, value);

            // This log is here for example sake. Do not send plaintext values to Logs
            _logger.LogInformation("Token key: {key} added to persistent store");

            await InsertTokenToCache(key, value);
        }

        #region Cache_Operations
        private async Task<Token> GetTokenFromCache(string key)
        {
            Token tokenResult = null;
            try
            {
                var valueBytes = await _cache.GetAsync(key);

                var tokenValue = Encoding.ASCII.GetString(valueBytes);

                // This log is here for example sake. Do not send plaintext values to Logs
                _logger.LogInformation($"Retrieved Token {key} from cache");

                tokenResult = new Token() { Key = key, Value = tokenValue };
            }
            catch (ArgumentNullException ae)
            {
                _logger.LogWarning(ae, $"Failed to retrieve key {key} from cache, attempting to retrieve from persistent storage");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to retrieve token from cache with Exception");
            }

            return tokenResult;
        }

        private async Task InsertTokenToCache(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return;
            }

            try
            {
                await _cache.SetAsync(key, Encoding.ASCII.GetBytes(value));
                _logger.LogInformation("Token key: {key} added to persistent cache");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown trying to set token into cache, see stack trace for details");
                return;
            }
        }
        #endregion

        #region PersistentStore_Operations
        private async Task<Token> GetTokenFromDb(string key)
        {
            var retrievedToken = await _tokenDb.GetToken(key);
            if (retrievedToken == null)
            {
                _logger.LogError("Could not retrieve token");
                return null;
            }

            if (string.IsNullOrEmpty(retrievedToken.Key) || string.IsNullOrEmpty(retrievedToken.Value))
            {
                // Add logs
                return null;
            }

            _logger.LogInformation("Retrieved token from persistent storage");
            return retrievedToken;
        }
        #endregion
    }
}