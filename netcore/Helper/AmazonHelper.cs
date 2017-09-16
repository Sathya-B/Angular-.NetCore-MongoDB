using System;
using Amazon.S3;
using Amazon.S3.Model;

namespace Arthur_Clive.Helper
{
    public class AmazonHelper
    {
        public static IAmazonS3 s3Client;
        public static string s3PrefixUrl = "https://s3.ap-south-1.amazonaws.com/";

        public static IAmazonS3 GetAmazonS3Client()
        {
            string accessKey = "AKIAIUAYVIL7A7I6XECA";
            string secretKey = "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";
            s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);
            return s3Client;
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
            string presignedUrl = s3PrefixUrl + bucketName + "/" + objectName;
            return presignedUrl;
        }
    }
}
