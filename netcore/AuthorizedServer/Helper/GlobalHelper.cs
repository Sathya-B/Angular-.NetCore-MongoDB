using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AuthorizedServer.Logger;
using MongoDB.Driver;
using MH = AuthorizedServer.Helper.MongoHelper;

namespace AuthorizedServer.Helper
{
    /// <summary>Global helper for authorized controller </summary>
    public class GlobalHelper
    {
        /// <summary>Client for MongoDB</summary>
        public MongoClient _client;
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

        /// <summary>To read XML</summary>
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

        /// <summary>Get ip config from xml</summary>
        public static string GetIpConfig()
        {
            try
            {
                var result = ReadXML().Elements("ipconfig").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("authorizedserver2");
                return result.First().Value;
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("GlobalHelper", "GetIpConfig", ex.Message, serverlogCollection);
                return null;
            }
        }
    }
}
