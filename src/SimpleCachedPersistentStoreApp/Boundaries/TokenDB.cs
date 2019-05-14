using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using SimpleCachedPersistentStoreApp.Models;

namespace SimpleCachedPersistentStoreApp.Boundaries
{
    public class TokenDB : ITokenDB
    {
        private readonly MySqlConnection _conn;
        private readonly ILogger<TokenDB> _logger;

        public TokenDB(MySqlConnection connection, ILogger<TokenDB> logger)
        {
            _logger = logger;
            _conn = connection;
        }

        public async Task InsertToken(string key, string value)
        {
            // DO NOT FORGET TO SANITIZE YOUR INPUTS!
            // WARNING: VULNERABLE TO SQL INJECTION ATTACKS!
            var sqlQuery = $"REPLACE INTO Tokens (TokenKey, TokenValue) VALUES ('{key}','{value}')";
            await ExecuteSqlCommand(sqlQuery);
            return;
        }

        public async Task<Token> GetToken(string key)
        {
            // DO NOT FORGET TO SANITIZE YOUR INPUTS!
            // WARNING: VULNERABLE TO SQL INJECTION ATTACKS!
            var sqlQuery = $"SELECT TokenKey, TokenValue FROM Tokens WHERE TokenKey='{key}'";
            return await ExecuteSqlCommand(sqlQuery);
        }

        public async Task DeleteToken(string key)
        {
            // DO NOT FORGET TO SANITIZE YOUR INPUTS!
            // WARNING: VULNERABLE TO SQL INJECTION ATTACKS!
            var sqlQuery = $"DELETE FROM `Tokens` WHERE `TokenKey` = {key}";
            await ExecuteSqlCommand(sqlQuery);
            return;
        }

        private async Task<Token> ExecuteSqlCommand(string query)
        {
            try
            {
                _conn.Open();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not connect to database");
                return null;
            }
            finally
            {
                _conn.Close();
            }

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, _conn);
                DbDataReader rdr = await cmd.ExecuteReaderAsync();

                Token result = null;
                while (rdr.Read())
                {
                    result = new Token();
                    result.Key = rdr[0].ToString();
                    result.Value = rdr[1].ToString();
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run MySQL Command");
                return null;
            }
            finally
            {
                _conn.Close();
            }
        }
    }
}