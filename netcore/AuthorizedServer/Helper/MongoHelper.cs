using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthorizedServer.Helper
{
    public class MongoHelper
    {
        public static IMongoDatabase _mongodb;

        public static MongoClient _client = GetClient();      

        public static MongoClient GetClient()
        {
            return new MongoClient("mongodb://localhost:27017");
        }

        public async Task<BsonDocument> GetSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName)
        {
            _mongodb = _client.GetDatabase(dbName);
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
            IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter);
            return cursor.FirstOrDefault();
        }

        public async Task<bool> UpdateSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName, UpdateDefinition<BsonDocument> update)
        {
            _mongodb = _client.GetDatabase(dbName);
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
           var cursor = await collection.UpdateOneAsync(filter, update);
            return cursor.ModifiedCount > 0;
        }

        //public bool CheckForDatas(string filterField1,string filterData1,string filterField2,string filterData2,string dbName,string collectionName)
        //{
        //    if(filterField2 == null)
        //    {
        //        var filter = Builders<BsonDocument>.Filter.Eq("PhoneNumber", filterData1);
        //    }
        //    else
        //    {

        //    }
        //    var verifyUser = helper.GetSingleObject(filter, "Authentication", "Authentication").Result;
        //}
    }
}
