using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Arthur_Clive.Helper
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

        public bool DeleteSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName)
        {
            var data = GetSingleObject(filter, dbName, collectionName).Result;
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
            var response = collection.DeleteOneAsync(data);
            return response.Result.DeletedCount > 0;
        }
    }
}
