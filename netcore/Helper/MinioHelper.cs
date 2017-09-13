using System;
using System.Threading.Tasks;
using Minio;

namespace Arthur_Clive.Helper
{
    public class MinioHelper
    {
        public static MinioClient GetMinioClient()
        {
            return new MinioClient("localhost:9000", "MinioServer", "123654789@Ragu");
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
