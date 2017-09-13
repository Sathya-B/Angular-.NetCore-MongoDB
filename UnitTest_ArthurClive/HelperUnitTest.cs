using System.Threading.Tasks;
using Amazon.S3;
using Arthur_Clive.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minio;
using AH = Arthur_Clive.Helper.AmazonHelper;

namespace UnitTest_ArthurClive
{
    [TestClass]
    public class HelperUnitTest
    {
        [TestMethod]
        public void Helper_Amazon_GetS3ObjectUrl()
        {
            //Arrange
            string bucketName = "sampleBucketName";
            string objectName = "sampleObjectName";
            //Act
            var result = AH.GetS3Object(bucketName,objectName) as string;
            //Assert
            Assert.AreEqual(AH.s3PrefixUrl + "sampleBucketName/sampleObjectName",result);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Helper_Amazon_GetAmazonS3ObjectPresignedUrl()
        {
            //Arrange
            string bucketName = "sampleBucketName";
            string objectName = "sampleObjectName";
            string presignedUrl = "https://s3.ap-south-1.amazonaws.com/sampleBucketName/sampleObjectName?X-Amz-Expires=300&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIUAYVIL7A7I6XECA/20170913/ap-south-1/s3/aws4_request&X-Amz-Date=";
            //Act
            var result = AH.GetAmazonS3Object(bucketName, objectName) as string;
            var subString = result.Split('=')[0] +""+ result.Split('=')[1];
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(presignedUrl,subString);
            
        }

        //[TestMethod]
        //public void Helper_Amazon_GetAmazonS3Client()
        //{
        //    //Arrange
        //    string accessKey = "AKIAIUAYVIL7A7I6XECA";
        //    string secretKey = "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";
        //    //Act
        //    var result = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.APSouth1);
        //    //Assert
        //    Assert.IsNotNull(result);
        //}

        //[TestMethod]
        //public void Helper_Minio_GetMinioClient()
        //{
        //    //Arrange
        //    string endPoint = "localhost:9000";
        //    string accessKey = "MinioServer";
        //    string secretKey = "123654789@Ragu";
        //    //Act
        //    var result = new MinioClient(endPoint, accessKey, secretKey); ;
        //    //Assert
        //    Assert.IsNotNull(result);
        //}

        //[TestMethod]
        //public void Helper_Minio_GetMinioObjectPresignedUrl()
        //{
        //    //Arrange
        //    string bucketName = "sampleBucketName";
        //    string objectName = "sampleObjectName";
        //    //Act
        //    var result = MinioHelper.GetMinioObject(bucketName, objectName) as Task<string>;
        //    //Assert
        //    Assert.IsNotNull(result);
        //}
    }
}
