using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Minio;

namespace Arthur_Clive.Helper
{
    public class MinioHelper
    {
        public static MinioClient GetMinioClient()
        {
            return new MinioClient(GlobalHelper.ReadXML().Elements("minioclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("host").First().Value,
                                    GlobalHelper.ReadXML().Elements("minioclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("accesskey").First().Value,
                                    GlobalHelper.ReadXML().Elements("minioclient").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("secretkey").First().Value);
        }

        public static async Task<string> GetMinioObject(string bucketName, string objectName)
        {
            try
            {
                string presignedUrl = await GetMinioClient().PresignedGetObjectAsync(bucketName, objectName, 1000);
                return presignedUrl;
            }
            catch(Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("AmazonHelper", "GetMinioObject", "GetMinioObject", ex.Message);
                return null;
            }
        }
    }
}
