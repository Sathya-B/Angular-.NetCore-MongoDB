using System.Threading.Tasks;
using AuthorizedServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using TH = UnitTest_AuthorizedServer.Controller.Integrationtest_AuthorizedServerController_Helper;
using MH = AuthorizedServer.Helper.MongoHelper;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace UnitTest_AuthorizedServer
{
    [TestClass]
    public class AuthController_IntegrationTest
    {
        [TestMethod]
        public void AuthController_Register_IntegrationTest_ArthurClive()
        {
            //Arrange
            var username = "12341234";
            RegisterModel registerModel = new RegisterModel
            {
                Title = "Mr",
                FullName = "SampleName",
                DialCode = "+91",
                PhoneNumber = "12341234",
                Email = "sample@gmail.com",
                Password = "asd123",
                UserLocation = "IN"
            };
            var expectedCode = "200";
            var expectedMessage = "User Registered";

            //Act
            var result = TH.GetAuthController().Register(registerModel) as Task<ActionResult>;
            var responseData = TH.DeserializedResponceData(result.Result.ToJson());

            //Check if user is unsubscribed
            var insertedData = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "Authentication", "Authentication").Result);

            //Assert
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(responseData.Code, expectedCode);
            Assert.AreEqual(responseData.Message, expectedMessage);
            Assert.AreEqual(insertedData.Title,registerModel.Title);
            Assert.AreEqual(insertedData.FullName, registerModel.FullName);
            Assert.AreEqual(insertedData.DialCode, registerModel.DialCode);
            Assert.AreEqual(insertedData.PhoneNumber, registerModel.PhoneNumber);
            Assert.AreEqual(insertedData.Email, registerModel.Email);
            Assert.AreEqual(insertedData.Password, registerModel.Password);
            Assert.AreEqual(insertedData.UserLocation, registerModel.UserLocation);
            Assert.AreEqual(insertedData.UserName, username);
            Assert.AreEqual(insertedData.UserRole, "User");
            Assert.AreEqual(insertedData.Title, registerModel.Title);
            Assert.IsNull(insertedData.SocialId);
            Assert.IsNotNull(insertedData.VerificationCode);
            Assert.AreEqual(insertedData.Status, "Registered");
            Assert.AreEqual(insertedData.WrongAttemptCount, 0);
        }
    }
}
