using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AuthorizedServer.Logger;
using AuthorizedServer.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MH = AuthorizedServer.Helper.MongoHelper;

namespace AuthorizedServer.Helper
{
    /// <summary>Global helper for authorized controller </summary>
    public class GlobalHelper
    {

        /// <summary>Get current directory of project</summary>
        public static string GetCurrentDir()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>To read XML</summary>
        public static XElement ReadXML()
        {
            var dir = GetCurrentDir();
            var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
            return XElement.Parse(xmlStr);
        }

        /// <summary>Get ip config from xml</summary>
        public static string GetIpConfig()
        {
            var result = ReadXML().Elements("ipconfig").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("authorizedserver2");
            return result.First().Value;
        }
    }
}
