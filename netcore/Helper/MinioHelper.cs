using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Arthur_Clive.Logger;
using Minio;
using MongoDB.Driver;
using MH = Arthur_Clive.Helper.MongoHelper;

namespace Arthur_Clive.Helper
{
    /// <summary>Helper method for Minio server</summary>
    public class MinioHelper
    {
        /// <summary></summary>
        public MongoClient _client;
        /// <summary></summary>
        public static IMongoDatabase logger_db;
        /// <summary></summary>
        public static IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary></summary>
        public MinioHelper()
        {
            _client = MH.GetClient();
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get Minio client</summary>
        public static MinioClient GetMinioClient()
        {
            try
            {
                return new MinioClient(GlobalHelper.ReadXML().Elements("minioclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("host").First().Value,
                                        GlobalHelper.ReadXML().Elements("minioclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("accesskey").First().Value,
                                        GlobalHelper.ReadXML().Elements("minioclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("secretkey").First().Value);
            }
            catch (Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("MinioHelper", "GetMinioClient", ex.Message, serverlogCollection);
                return null;
            }
        }

        /// <summary>Get Minio object presigned url</summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        public static async Task<string> GetMinioObject(string bucketName, string objectName)
        {
            try
            {
                string presignedUrl = await GetMinioClient().PresignedGetObjectAsync(bucketName, objectName, 1000);
                return presignedUrl;
            }
            catch (Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("MinioHelper", "GetMinioObject", ex.Message, serverlogCollection);
                return null;
            }
        }
    }
}
