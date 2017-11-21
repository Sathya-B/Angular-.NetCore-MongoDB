using System.Linq;
using MH = AuthorizedServer.Helper.MongoHelper;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace AuthorizedServer.Repositories
{
    /// <summary>Repository for Token</summary>
    public class RTokenRepository : IRTokenRepository
    {
        /// <summary>Client for MongoDB</summary>
        public MongoClient _client;
        /// <summary></summary>
        public IMongoDatabase token_db;
        /// <summary></summary>
        public IMongoCollection<RToken> rTokenCollection;

        /// <summary></summary>
        public RTokenRepository()
        {
            _client = MH.GetClient();
            token_db = _client.GetDatabase("TokenDB");
            rTokenCollection = token_db.GetCollection<RToken>("RToken");
        }

        /// <summary>Add token</summary>
        /// <param name="token"></param>
        public async Task<bool> AddToken(RToken token)
        {
            try
            {
                await rTokenCollection.InsertOneAsync(token);
                return true;
            }
            catch
            {
                return false;
            }             
        }
        /// <summary>Expire token</summary>
        /// <param name="token"></param>
        public async Task<bool> ExpireToken(RToken token)
        {       
            var filter = Builders<RToken>.Filter.Eq("client_id", token.ClientId) & Builders<RToken>.Filter.Eq("refresh_token", token.RefreshToken); ;
            var update = Builders<RToken>.Update.Set("isstop", token.IsStop);
            var result = await rTokenCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        /// <summary>Get token</summary>
        /// <param name="refresh_token"></param>
        /// <param name="client_id"></param>
        public async Task<RToken> GetToken(string refresh_token, string client_id)
        {                    
            var filter = "{ client_id: '" + client_id + "' , refresh_token: '" + refresh_token+ "'}";
            IAsyncCursor<RToken> cursor = await rTokenCollection.FindAsync(filter);
            return cursor.FirstOrDefault();
        }
    }
}
