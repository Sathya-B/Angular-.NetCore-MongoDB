using System.Threading.Tasks;
using AuthorizedServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using TH = UnitTest_AuthorizedServer.Controller.Integrationtest_AuthorizedServerController_Helper;
using MH = AuthorizedServer.Helper.MongoHelper;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using AuthorizedServer;
using System;
using Microsoft.AspNetCore.Identity;

namespace UnitTest_AuthorizedServer
{
    [TestClass]
    public class AuthController_IntegrationTest
    {
        public PasswordHasher<VerificationModel> smsHasher = new PasswordHasher<VerificationModel>();
        [TestMethod]
        public void AuthController_Register_IntegrationTest_AuthorizedServer()
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
            Assert.AreEqual(insertedData.Title, registerModel.Title);
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

            //Delete inserted test data
            var checkData = MH.CheckForDatas("UserName", username, null, null, "Authentication", "Authentication");
            if (checkData != null)
            {
                var delete = MH.DeleteSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "Authentication", "Authentication");
            }
        }

        //[TestMethod]
        public void AuthController_RegisterVerification_IntegrationTest_AuthorizedServer()
        {
            //Arrange
            var username = "12341234";
            var otp = "123456";
            VerificationModel verificationModel = new VerificationModel
            {
                UserName = username,
                VerificationCode = otp
            };
            var verificationCode = smsHasher.HashPassword(verificationModel,otp);
            RegisterModel registerModel = new RegisterModel
            {
                Title = "Mr",
                FullName = "SampleName",
                UserName = username,
                UserRole = "User",
                DialCode = "+91",
                PhoneNumber = "12341234",
                Email = "sample@gmail.com",
                Password = "SamplePassword",
                UserLocation = "IN",
                Status = "Registered",
                OTPExp = DateTime.UtcNow.AddMinutes(4),
                VerificationCode = verificationCode
            };

            //Act
            var insert = TH.InsertRegiterModeldata(registerModel).Result;
            var result = TH.GetAuthController().RegisterVerification(username,otp) as ActionResult;
            var responseData = TH.DeserializedResponceData(result.ToJson());

            //Check if user is unsubscribed
            var insertedData = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "Authentication", "Authentication").Result);
            
            //Assert
            Assert.IsNotNull(result);

            //Delete inserted test data
            var checkData = MH.CheckForDatas("UserName", username, null, null, "Authentication", "Authentication");
            if (checkData != null)
            {
                var delete = MH.DeleteSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "Authentication", "Authentication");
            }
        }
    }

    [TestClass]
    public class TokenController_IntegrationTest
    {
        //[TestMethod]
        public void TokenController_Auth_IntegrationTest_AuthorizedServer()
        {
            //Arrange
            Parameters parameters1 = new Parameters
            {
                grant_type = "password",
                username = "SampleUser1",
                fullname = "SampleName1"
            };
            var expectedCode1 = "200";
            var expectedMessage1 = "User Registered";

            //Act

            var result1 = TH.GetTokenController().Auth(parameters1) as ActionResult;
            var responseData1 = TH.DeserializedResponceData(result1.ToJson());
            Parameters parameters2 = new Parameters
            {
                grant_type = "refresh_token",
                username = parameters1.username,
                fullname = parameters1.fullname
            };
            var result2 = TH.GetTokenController().Auth(parameters2) as ActionResult;
            var responseData2 = TH.DeserializedResponceData(result2.ToJson());

            //Check if user is unsubscribed
            //var insertedData = BsonSerializer.Deserialize<RegisterModel>(MH.GetSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", username), "Authentication", "Authentication").Result);

            //Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
        }
    }
}
