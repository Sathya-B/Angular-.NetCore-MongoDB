using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Helper
{
    /// <summary>Global helper method</summary>
    public class GlobalHelper
    {
        /// <summary></summary>
        public static MongoClient _client;
        /// <summary></summary>
        public static IMongoDatabase logger_db;
        /// <summary></summary>
        public static IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary></summary>
        public GlobalHelper()
        {
            _client = MH.GetClient();
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get current directory of project</summary>
        public static string GetCurrentDir()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>xml file</summary>
        public static XElement ReadXML()
        {
            try
            {
                var dir = GetCurrentDir();
                var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
                return XElement.Parse(xmlStr);
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "ReadXML", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Send gift through email after payment success</summary>
        /// <param name="orderId"></param>
        /// <param name="orderinfo_collection"></param>
        public static string SendGift(long orderId, IMongoCollection<BsonDocument> orderinfo_collection)
        {
            try
            {
                var checkOrder = MH.GetSingleObject(orderinfo_collection, "OrderId", orderId, null, null).Result;
                if (checkOrder != null)
                {
                    var orderInfo = BsonSerializer.Deserialize<OrderInfo>(checkOrder);
                    List<string> productInfoList = new List<string>();
                    foreach (var product in orderInfo.ProductDetails)
                    {
                        productInfoList.Add(product.ProductSKU);
                    }
                    var productInfoString = String.Join(":", productInfoList);
                    var sendGift = EmailHelper.SendGift(orderId, productInfoString);
                }
                return "Success";
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "SendGift", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Get string between to characters</summary>
        /// <param name="text"></param>
        /// <param name="startString"></param>
        /// <param name="endString"></param>
        public static List<string> StringBetweenTwoCharacters(string text, string startString, string endString)
        {
            try
            {
                List<string> matched = new List<string>();
                int indexStart = 0, indexEnd = 0;
                bool exit = false;
                while (!exit)
                {
                    indexStart = text.IndexOf(startString);
                    indexEnd = text.IndexOf(endString);
                    if (indexStart != -1 && indexEnd != -1)
                    {
                        matched.Add(text.Substring(indexStart + startString.Length,
                            indexEnd - indexStart - startString.Length));
                        text = text.Substring(indexEnd + endString.Length);
                    }
                    else
                        exit = true;
                }
                return matched;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "StringBetweenTwoCharacters", ex.Message, serverlogCollection);
                return null;
            }
        }
    }
}
