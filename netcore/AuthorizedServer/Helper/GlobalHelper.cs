using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AuthorizedServer.Helper
{
    public class GlobalHelper
    {
        public static XElement ReadXML()
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
            return XElement.Parse(xmlStr);
        }

        public static string GetIpConfig()
        {
            var result = ReadXML().Elements("ipconfig").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("authorizedserver2");
            return result.First().Value;
        }
    }
}
