using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
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
            return new MongoClient(GlobalHelper.ReadXML().Elements("mongoclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("host").First().Value);
        }

        public static async Task<BsonDocument> GetSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName)
        {
            _mongodb = _client.GetDatabase(dbName);
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
            IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter);
            return cursor.FirstOrDefault();
        }

        public static async Task<bool> UpdateSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName, UpdateDefinition<BsonDocument> update)
        {
            _mongodb = _client.GetDatabase(dbName);
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
           var cursor = await collection.UpdateOneAsync(filter, update);
            return cursor.ModifiedCount > 0;
        }

        public static BsonDocument CheckForDatas(string filterField1, string filterData1, string filterField2, string filterData2, string dbName, string collectionName)
        {
            FilterDefinition<BsonDocument> filter;
            if (filterField2 == null)
            {
                filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
            }
            else
            {
                filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
            }
            return GetSingleObject(filter, dbName, collectionName).Result;
        }
    }
}
