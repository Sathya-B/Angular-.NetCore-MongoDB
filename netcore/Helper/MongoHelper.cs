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
using MongoDB.Bson.Serialization;

namespace Arthur_Clive.Helper
{
    /// <summary>Helper method for MongoDB</summary>
    public class MongoHelper
    {
        /// <summary>Client for MongoDB</summary>
        public static MongoClient _client;
        /// <summary></summary>
        public static FilterDefinition<BsonDocument> filter;
        /// <summary>
        /// 
        /// </summary>
        public static IMongoDatabase product_db;
        /// <summary></summary>
        public static IMongoDatabase userinfo_db;
        /// <summary>
        /// 
        /// </summary>
        public static IMongoCollection<BsonDocument> cart_collection;
        /// <summary></summary>
        public static IMongoDatabase logger_db;
        /// <summary></summary>
        public static IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary>
        /// 
        /// </summary>
        public MongoHelper()
        {
            _client = GetClient();
            product_db = _client.GetDatabase("ProductDB");
            userinfo_db = _client.GetDatabase("UserInfo");
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get client for MongoDB</summary>
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
        /// <param name="collection"></param>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
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
        /// <param name="collection"></param>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
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

        /// <summary>Update single object in MongoDB </summary>
        /// <param name="collection"></param>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        /// <param name="update"></param>
        public static async Task<bool?> UpdateSingleObject(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2, UpdateDefinition<BsonDocument> update)
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
                var cursor = await collection.UpdateOneAsync(filter, update);
                if (cursor.ModifiedCount > 0)
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
                LoggerDataAccess.CreateLog("MongoHelper", "UpdateSingleObject", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Delete single object from MongoDB</summary>
        /// <param name="collection"></param>
        /// <param name="filterField1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterData2"></param>
        public static bool? DeleteSingleObject(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2)
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
                var response = collection.DeleteOneAsync(filter);
                return response.Result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "DeleteSingleObject", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Check if a data is present in MongoDB</summary>
        /// <param name="collection"></param>
        /// <param name="filterField1"></param>
        /// <param name="filterData1"></param>
        /// <param name="filterField2"></param>
        /// <param name="filterData2"></param>
        public static bool? CheckForDatas(IMongoCollection<BsonDocument> collection, string filterField1, dynamic filterData1, string filterField2, dynamic filterData2)
        {
            try
            {
                var result = GetSingleObject(collection, filterField1, filterData1, filterField2, filterData2).Result;
                if (result != null)
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
                LoggerDataAccess.CreateLog("MongoHelper", "CheckForDatas", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary></summary>
        /// <param name="collection"></param>
        /// <param name="objectId"></param>
        /// <param name="updateData"></param>
        /// <param name="updateField"></param>
        /// <param name="objectName"></param>
        public async static Task<bool?> UpdateProductDetails(IMongoCollection<BsonDocument> collection, dynamic objectId, dynamic updateData, string updateField, string objectName)
        {
            try
            {
                var update = await MH.UpdateSingleObject(collection, "_id", objectId, null, null, Builders<BsonDocument>.Update.Set(updateField, updateData));
                string MinioObject_URL;
                //MinioObject_URL = WH.GetMinioObject("arthurclive-products", objectName).Result;
                //MinioObject_URL = AH.GetAmazonS3Object("arthurclive-products", objectName);
                MinioObject_URL = AH.GetS3Object("arthurclive-products", objectName);
                var update1 = await MH.UpdateSingleObject(collection, "_id", objectId, null, null, Builders<BsonDocument>.Update.Set("ProductSKU", objectName));
                var update2 = await MH.UpdateSingleObject(collection, "_id", objectId, null, null, Builders<BsonDocument>.Update.Set("MinioObject_URL", MinioObject_URL));
                if (update1 == true & update2 == true)
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
                LoggerDataAccess.CreateLog("MongoHelper", "UpdateProductDetails", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary></summary>
        /// <param name="collection"></param>
        /// <param name="objectId"></param>
        /// <param name="productFor"></param>
        /// <param name="productType"></param>
        /// <param name="updateData"></param>
        /// <param name="updateField"></param>
        /// <param name="objectName"></param>
        public async static Task<bool?> UpdateCategoryDetails(IMongoCollection<BsonDocument> collection, dynamic objectId, string productFor, string productType, dynamic updateData, string updateField, string objectName)
        {
            try
            {
                var update = await MH.UpdateSingleObject(collection, "_id", objectId, null, null, Builders<BsonDocument>.Update.Set(updateField, updateData));
                string MinioObject_URL;
                //MinioObject_URL = WH.GetMinioObject("products", objectName).Result;
                //MinioObject_URL = AH.GetAmazonS3Object("product-category", objectName);
                MinioObject_URL = AH.GetS3Object("product-category", objectName);
                var update1 = await MH.UpdateSingleObject(collection, "_id", objectId, null, null, Builders<BsonDocument>.Update.Set("MinioObject_URL", MinioObject_URL));
                return update1;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "UpdateCategoryDetails", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Update order details</summary>
        /// <param name="collection"></param>
        /// <param name="orderId"></param>
        public async static Task<string> UpdatePaymentDetails(IMongoCollection<BsonDocument> collection, long orderId)
        {
            try
            {
                PaymentMethod paymentDetails = new PaymentMethod();
                List<StatusCode> statusCodeList = new List<StatusCode>();
                var orderData = BsonSerializer.Deserialize<OrderInfo>(MH.GetSingleObject(collection, "OrderId", orderId, null, null).Result);
                foreach (var detail in orderData.PaymentDetails.Status)
                {
                    statusCodeList.Add(detail);
                }
                statusCodeList.Add(new StatusCode { StatusId = 2, Description = "Payment Received", Date = DateTime.UtcNow });
                paymentDetails.Status = statusCodeList;
                var updatePaymentDetails = await MH.UpdateSingleObject(collection, "OrderId", orderId, null, null, Builders<BsonDocument>.Update.Set("PaymentDetails", paymentDetails));
                return "Success";
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "UpdatePaymentDetails", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Remove product in a particular cart</summary>
        /// <param name="collection"></param>
        /// <param name="orderId"></param>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="cartCollection"></param>
        /// <param name="productCollection"></param>
        public async static Task<string> RemoveCartItems(IMongoCollection<BsonDocument> collection, IMongoCollection<Cart> cartCollection, IMongoCollection<BsonDocument> productCollection, long orderId, string userName, string email)
        {
            try
            {
                IAsyncCursor<Cart> cartCursor = await cartCollection.FindAsync(Builders<Cart>.Filter.Eq("UserName", userName));
                var cartDatas = cartCursor.ToList();
                foreach (var cart in cartDatas)
                {
                    var product = BsonSerializer.Deserialize<Product>(GetSingleObject(productCollection, "ProductSKU", cart.ProductSKU, null, null).Result);
                    long updateQuantity = product.ProductStock - cart.ProductQuantity;
                    if (product.ProductStock - cart.ProductQuantity < 0)
                    {
                        updateQuantity = 0;
                        var emailResponce = EmailHelper.SendEmailToAdmin(userName.ToString(), email.ToString(), cart.ProductSKU, cart.ProductQuantity, product.ProductStock, orderId).Result;
                    }
                    var result = MH.UpdateSingleObject(collection, "ProductSKU", cart.ProductSKU, null, null, Builders<BsonDocument>.Update.Set("ProductStock", updateQuantity)).Result;
                }
                var removeCartItems = cartCollection.DeleteMany(Builders<Cart>.Filter.Eq("UserName", userName));
                return "Success";
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("MongoHelper", "RemoveCartItems", ex.Message, serverlogCollection);
                return null;
            }
        }
    }
}
