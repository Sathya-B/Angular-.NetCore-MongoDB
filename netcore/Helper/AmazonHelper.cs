using System;
using Amazon.S3;
using Amazon.S3.Model;

namespace Arthur_Clive.Helper
{
    public class AmazonHelper
    {
        public static IAmazonS3 s3Client;

        public static void GetAmazonS3Client()
        {
            try
            {
                string accessKey = "AKIAIUAYVIL7A7I6XECA";
                string secretKey = "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";
                s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);
            }
            catch (Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("AmazonHelper", "GetAmazonS3Client", "GetAmazonS3Client", ex.Message);
            }
        }

        public static string GetAmazonS3Object(string bucketName, string objectKey)
        {
            try
            {
                GetAmazonS3Client();
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.Now.AddMinutes(5)
                };
                string url = s3Client.GetPreSignedURL(request);
                return url;
            }
            catch (Exception ex)
            {
                Logger.LoggerDataAccess.CreateLog("AmazonHelper", "GetAmazonS3Object", "GetAmazonS3Object", ex.Message);
                return "";
            }
        }

        public static string GetS3Object(string bucketName, string objectName)
        {
            string s3 = "https://s3.ap-south-1.amazonaws.com/";
            string presignedUrl = s3 + bucketName + "/" + objectName;
            return presignedUrl;
        }
    }
}
