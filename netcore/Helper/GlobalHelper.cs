using System.IO;
using System.Xml.Linq;

namespace Arthur_Clive.Helper
{
    /// <summary>Global helper method</summary>
    public class GlobalHelper
    {
        /// <summary>Get current directory of project</summary>
        public static string GetCurrentDir()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>xml file</summary>
        public static XElement ReadXML()
        {
            var dir = GetCurrentDir();
            var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
            return XElement.Parse(xmlStr);
        }
    }
}
