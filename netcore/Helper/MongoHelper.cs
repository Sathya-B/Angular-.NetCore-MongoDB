using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using AH = Arthur_Clive.Helper.AmazonHelper;
using WH = Arthur_Clive.Helper.MinioHelper;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Helper
{
    /// <summary>Helper method for MongoDB</summary>
    public class MongoHelper
    {
        /// <summary></summary>
        public static IMongoDatabase _mongodb;
        /// <summary>Client for MongoDB</summary>
        public static MongoClient _client = GetClient();
        /// <summary></summary>
        public static FilterDefinition<BsonDocument> filter;
        /// <summary>Get client for MongoDB</summary>
        public static MongoClient GetClient()
        {
            var ip = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("ip").First().Value;
            var user = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("user").First().Value;
            var password = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("password").First().Value;
            var db = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("db").First().Value;
            var connectionString = "mongodb://"+user+":"+password+"@"+ip+":27017/"+ db;
            var mongoClient = new MongoClient(connectionString);
            return mongoClient;
        }

        /// <summary>Get single object from MongoDB</summary>
        /// <param name="filter"></param>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        public static async Task<BsonDocument> GetSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName)
        {
            _mongodb = _client.GetDatabase(dbName);
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
            IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter);
            return cursor.FirstOrDefault();
        }

        /// <summary>Get list of objects from MongoDB</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterField3"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="filterData3"></param>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        public static async Task<List<BsonDocument>> GetListOfObjects(string filterField1, dynamic filterData1, string filterField2, dynamic filterData2, string filterField3, dynamic filterData3, string dbName, string collectionName)
        {
            try
            {
                var db = _client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);
                if (filterField1 == null & filterField2 == null & filterField3 == null)
                {
                    filter = FilterDefinition<BsonDocument>.Empty;
                }
                else if (filterField1 != null & filterField2 == null & filterField3 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1);
                }
                else if (filterField1 != null & filterField2 != null & filterField3 == null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2);
                }
                else if (filterField1 != null & filterField2 != null & filterField3 != null)
                {
                    filter = Builders<BsonDocument>.Filter.Eq(filterField1, filterData1) & Builders<BsonDocument>.Filter.Eq(filterField2, filterData2) & Builders<BsonDocument>.Filter.Eq(filterField3, filterData3);
                }
                IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(filter);
                return cursor.ToList();
            }
            catch(Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "GetListOfObjects", "GetListOfObjects", ex.Message);
                return null;
            }
        }

        /// <summary>Update single object in MongoDB </summary>
        /// <param name="filter"></param>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="update"></param>
        public static async Task<bool> UpdateSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName, UpdateDefinition<BsonDocument> update)
        {
            _mongodb = _client.GetDatabase(dbName);
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
            var cursor = await collection.UpdateOneAsync(filter, update);
            return cursor.ModifiedCount > 0;
        }

        /// <summary>Delete single object from MongoDB</summary>
        /// <param name="filter"></param>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        public static bool DeleteSingleObject(FilterDefinition<BsonDocument> filter, string dbName, string collectionName)
        {
            var data = GetSingleObject(filter, dbName, collectionName).Result;
            var collection = _mongodb.GetCollection<BsonDocument>(collectionName);
            var response = collection.DeleteOneAsync(data);
            return response.Result.DeletedCount > 0;
        }

        /// <summary>Check if a data is present in MongoDB</summary>
        /// <param name="filterField1"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData2"></param>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        public static BsonDocument CheckForDatas(string filterField1, dynamic filterData1, string filterField2, dynamic filterData2, string dbName, string collectionName)
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

        /// <summary>Get product list  from MongoDB</summary>
        /// <param name="productSKU"></param>
        /// <param name="product_db"></param>
        public async static Task<List<Product>> GetProducts(string productSKU, IMongoDatabase product_db)
        {
            try
            {
                IAsyncCursor<Product> productCursor = await product_db.GetCollection<Product>("Product").FindAsync(Builders<Product>.Filter.Eq("ProductSKU", productSKU));
                var products = productCursor.ToList();
                return products;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "GetProducts", "GetProducts", ex.Message);
                return null;
            }
        }

        /// <summary></summary>
        /// <param name="objectId"></param>
        /// <param name="updateData"></param>
        /// <param name="updateField"></param>
        /// <param name="objectName"></param>
        public async static Task<bool> UpdateProductDetails(dynamic objectId, dynamic updateData, string updateField, string objectName)
        {
            try
            {
                var update = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("_id", objectId), "ProductDB", "Product", Builders<BsonDocument>.Update.Set(updateField, updateData));
                string MinioObject_URL;
                //MinioObject_URL = WH.GetMinioObject("arthurclive-products", objectName).Result;
                //MinioObject_URL = AH.GetAmazonS3Object("arthurclive-products", objectName);
                MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                var update1 = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("_id", objectId), "ProductDB", "Product", Builders<BsonDocument>.Update.Set("ProductSKU", objectName));
                var update2 = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("_id", objectId), "ProductDB", "Product", Builders<BsonDocument>.Update.Set("MinioObject_URL", MinioObject_URL));
                if(update1 == true & update2 == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "UpdateProductDetails", "UpdateProductDetails", ex.Message);
                return false;
            }
        }

        /// <summary></summary>
        /// <param name="objectId"></param>
        /// <param name="productFor"></param>
        /// <param name="productType"></param>
        /// <param name="updateData"></param>
        /// <param name="updateField"></param>
        /// <param name="objectName"></param>
        public async static Task<bool> UpdateCategoryDetails(dynamic objectId, string productFor, string productType, dynamic updateData, string updateField, string objectName)
        {
            try
            {
                var update = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("_id", objectId), "ProductDB", "Category", Builders<BsonDocument>.Update.Set(updateField, updateData));
                string MinioObject_URL;
                //MinioObject_URL = WH.GetMinioObject("products", objectName).Result;
                //MinioObject_URL = AH.GetAmazonS3Object("product-category", objectName);
                MinioObject_URL = AH.GetS3Object("product-category", objectName);
                var update1 = await MH.UpdateSingleObject(Builders<BsonDocument>.Filter.Eq("_id", objectId), "ProductDB", "Category", Builders<BsonDocument>.Update.Set("MinioObject_URL", MinioObject_URL));
                return update1;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "UpdateCategoryDetails", "UpdateCategoryDetails", ex.Message);
                return false;
            }
        }

    }
}
