using System;
using System.Threading.Tasks;
using Minio;
using MongoDB.Driver;

namespace Arthur_Clive.Helper
{
    public class WebApiHelper
    {       
        public static MongoClient GetClient()
        {            
            return new MongoClient("mongodb://localhost:27017");
        }

        public static MinioClient GetMinioClient()
        {
            return new MinioClient("localhost:9000", "MinioServer", "123654789@Ragu");
        }

        public static async Task<string> GetMinioObject(MinioClient minio, string bucketName, string objectName)
        {
            string presignedUrl = await minio.PresignedGetObjectAsync(bucketName, objectName, 1000);
            return presignedUrl;
        }
    }
}
