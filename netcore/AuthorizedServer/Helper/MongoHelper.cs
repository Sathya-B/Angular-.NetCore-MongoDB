using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AuthorizedServer.Logger;
using AuthorizedServer.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MH = AuthorizedServer.Helper.MongoHelper;

namespace AuthorizedServer.Helper
{
    /// <summary>Helper for MongoDB operations</summary>
    public class MongoHelper
    {
        /// <summary>Client for MongoDB</summary>
        public static MongoClient _client;
        /// <summary></summary>
        public static FilterDefinition<BsonDocument> filter;
        /// <summary></summary>
        public static IMongoDatabase logger_db;
        /// <summary></summary>
        public static IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary></summary>
        public MongoHelper()
        {
            _client = GetClient();
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get Mongo Client</summary>
        public static MongoClient GetClient()
        {
            try
            {
                var ip = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("ip").First().Value;
                var user = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("user").First().Value;
                var password = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("password").First().Value;
                var db = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("db").First().Value;
                var connectionString = "mongodb://" + user + ":" + password + "@" + ip + ":27017/" + db;
                var mongoClient = new MongoClient(connectionString);
                return mongoClient;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "GetClient", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Get single object from MongoDB</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="collection"></param>
        public static async Task<BsonDocument> GetSingleObject(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2)
        {
            try
            {
                if (filterField1 == null & filterField2 == null)
                {
                    filter = FilterDefinition<BsonDocument>.Empty;
                }
                else if (filterField1 != null & filterField2 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
                }
                else if (filterField1 != null & filterField2 != null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
                }
                IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter);
                return cursor.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "GetSingleObject", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Get list of objects from MongoDB</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="collection"></param>
        public static async Task<List<BsonDocument>> GetListOfObjects(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2)
        {
            try
            {
                if (filterField1 == null & filterField2 == null)
                {
                    filter = FilterDefinition<BsonDocument>.Empty;
                }
                else if (filterField1 != null & filterField2 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
                }
                else if (filterField1 != null & filterField2 != null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
                }
                IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter);
                return cursor.ToList();
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "GetListOfObjects", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Update single object in MongoDB</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="collection"></param>
        /// <param name="update"></param>
        public static async Task<bool?> UpdateSingleObject(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2, UpdateDefinition<BsonDocument> update)
        {
            try
            {
                if (filterField1 == null & filterField2 == null)
                {
                    filter = FilterDefinition<BsonDocument>.Empty;
                }
                else if (filterField1 != null & filterField2 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
                }
                else if (filterField1 != null & filterField2 != null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
                }
                var cursor = await collection.UpdateOneAsync(filter, update);
                return cursor.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "UpdateSingleObject", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>
        /// Chech MongoDB for specific data
        /// </summary>
        /// <param name="filterField1"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData2"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static BsonDocument CheckForDatas(IMongoCollection<BsonDocument> collection, string filterField1, string filterData1, string filterField2, string filterData2)
        {
            try
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
                return GetSingleObject(collection, filterField1, filterData1, filterField2, filterData2).Result;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "CheckForDatas", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>To record invalid login attempts</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="collection"></param>
        public static string RecordLoginAttempts(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2)
        {
            try
            {
                if (filterField1 == null & filterField2 == null)
                {
                    filter = FilterDefinition<BsonDocument>.Empty;
                }
                else if (filterField1 != null & filterField2 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
                }
                else if (filterField1 != null & filterField2 != null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
                }
                var verifyUser = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(collection, filterField1, filterData1, filterField2, filterData2).Result);
                if (verifyUser.WrongAttemptCount < 10)
                {
                    var update = Builders<BsonDocument>.Update.Set("WrongAttemptCount", verifyUser.WrongAttemptCount + 1);
                    var result = MH.UpdateSingleObject(collection, filterField1, filterData1, filterField2, filterData2, update).Result;
                    return "Login Attempt Recorded";
                }
                else
                {
                    var update = Builders<BsonDocument>.Update.Set("Status", "Revoked");
                    var result = MH.UpdateSingleObject(collection, filterField1, filterData1, filterField2, filterData2, update).Result;
                    return "Account Blocked";
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "RecordLoginAttempts", ex.Message, serverlogCollection);
                return "Failed";
            }
        }

        /// <summary>Delete single object from MongoDB</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="collection"></param>
        public static bool? DeleteSingleObject(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2)
        {
            try
            {
                if (filterField1 == null & filterField2 == null)
                {
                    filter = FilterDefinition<BsonDocument>.Empty;
                }
                else if (filterField1 != null & filterField2 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
                }
                else if (filterField1 != null & filterField2 != null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
                }
                var response = collection.DeleteOneAsync(filter);
                return response.Result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "DeleteSingleObject", ex.Message, serverlogCollection);
                return null;
            }
        }
    }
}
