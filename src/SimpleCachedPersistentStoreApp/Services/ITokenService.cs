using System.Threading.Tasks;
using SimpleCachedPersistentStoreApp.Models;

namespace SimpleCachedPersistentStoreApp.Services
{
    public interface ITokenService
    {
        Task DeleteToken(string key);
        Task InsertToken(string key, string value);
        Task<Token> GetToken(string key);
    }
}