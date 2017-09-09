using System.Linq;
using WH = AuthorizedServer.Helper.MongoHelper;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AuthorizedServer.Repositories
{

    public class RTokenRepository : IRTokenRepository
    {
        public IMongoDatabase _db;
        public RTokenRepository()
        {
           _db = WH._client.GetDatabase("TokenDB");
        }
        public async Task<bool> AddToken(RToken token)
        {
            var collection = _db.GetCollection<RToken>("RToken");
            try
            {
                await collection.InsertOneAsync(token);
                return true;
            }
            catch
            {
                return false;
            }             
        }

        public async Task<bool> ExpireToken(RToken token)
        {       
            var filter = Builders<RToken>.Filter.Eq("client_id", token.ClientId) & Builders<RToken>.Filter.Eq("refresh_token", token.RefreshToken); ;
            var update = Builders<RToken>.Update.Set("isstop", token.IsStop);
            var collection = _db.GetCollection<RToken>("RToken");
            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<RToken> GetToken(string refresh_token, string client_id)
        {                    
            var filter = "{ client_id: '" + client_id + "' , refresh_token: '" + refresh_token+ "'}";
            var collection = _db.GetCollection<RToken>("RToken");
            IAsyncCursor<RToken> cursor = await collection.FindAsync(filter);
            return cursor.FirstOrDefault();
        }
    }
}
