using System.IO;
using System.Xml.Linq;

namespace Arthur_Clive.Helper
{
    public class GlobalHelper
    {
        public static XElement ReadXML()
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
            return XElement.Parse(xmlStr);
        }
    }
}
