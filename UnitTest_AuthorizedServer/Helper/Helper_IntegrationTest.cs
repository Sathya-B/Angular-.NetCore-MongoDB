using AuthorizedServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AuthorizedServer.Helper;
using AuthorizedServer.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using SH = AuthorizedServer.Helper.SMSHelper;
using System;
using System.Linq;
using System.Xml.Linq;
using MongoDB.Driver;

namespace UnitTest_AuthorizedServer.Helper
{
    [TestClass]
    public class AuthHelper_IntegrationTest
    {
        public AuthHelper authHelper = new AuthHelper();

        //Pending
        //[TestMethod]
        public void DoPassword()
        {
            //Arrange
            var repo = new Mock<IRTokenRepository>();
            var settings = new Mock<IOptions<Audience>>();
            Parameters parameters = new Parameters();
            parameters.username = "sample@gmail.com";
            parameters.fullname = "Sample User";

            //Act
            var result = authHelper.DoPassword(parameters, repo.Object, settings.Object);

            //Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual("999", result.Code);
            //Assert.IsNotNull(result.Data);
            //Assert.AreEqual("Ok", result.Message);
        }

        //Pending
        //[TestMethod]
        public void DoRefreshToken()
        {
            //Arrange
            var repo = new Mock<IRTokenRepository>();
            var settings = new Mock<IOptions<Audience>>();
            Parameters parameters = new Parameters();
            parameters.username = "sample@gmail.com";
            parameters.fullname = "Sample User";

            //Act
            var result = authHelper.DoRefreshToken(parameters, repo.Object, settings.Object);

            //Assert
            Assert.IsNotNull(result);
            //Assert.AreEqual("999", result.Code);
            //Assert.IsNotNull(result.Data);
            //Assert.AreEqual("Ok", result.Message);
        }

        //Pending
        //[TestMethod]
        public void GetJWT()
        {
            //Arrange
            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");
            var client_id = "sample@gmail.com";
            var roleName = "";
            var settings = new Mock<IOptions<Audience>>();

            //Act
            var result = authHelper.GetJwt(client_id, refresh_token, settings.Object, roleName) as string;

            //Assert
            Assert.IsNotNull(result);
        }
    }

    [TestClass]
    public class EmailHelper_IntegrationTest
    {
        [TestMethod]
        public void EmailHelper_GetCredentials()
        {
            //Arrange
            var key1 = "accesskey";
            var key2 = "secretkey";

            //Act
            var accessKey = EmailHelper.GetCredentials(key1) as string;
            var secretKey = EmailHelper.GetCredentials(key2) as string;
            var accesskeyFromXML = GlobalHelper.ReadXML().Elements("amazonses").Where(x => x.Element("current").Value.Equals("Yes")).Descendants(key1);
            var secretkeyFromXML = GlobalHelper.ReadXML().Elements("amazonses").Where(x => x.Element("current").Value.Equals("Yes")).Descendants(key2);

            //Assert
            Assert.IsNotNull(accessKey);
            Assert.IsNotNull(secretKey);
            Assert.IsNotNull(accesskeyFromXML);
            Assert.IsNotNull(secretkeyFromXML);
        }

        //Pending
        //[TestMethod]
        public void EmailHelper_SendEmail()
        {
            //Arrange

            //Act

            //Assert
        }

        [TestMethod]
        public void EmailHelper_CreateEmailBody()
        {
            //Arrange
            var linkFromXML = GlobalHelper.ReadXML().Elements("ipconfig").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("authorizedserver2");
            var fullname = "Sample user";
            var link = "<a href ='" + GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("websitelink").First().Value + "'>Click Here</a>";
            var expectedEmail = "<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n\r\n<head>\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n    <title>Confirmation Email from Team ArthurClive</title>\r\n</head>\r\n\r\n<body>\r\n    <table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n        <tr>\r\n            <td align=\"center\" valign=\"top\" bgcolor=\"#ffe77b\" style=\"background-color:#dec786;\">\r\n                <br>\r\n                <br>\r\n                <table width=\"600\" height=\"150\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\r\n                    <tr>\r\n                        <td align=\"left\" valign=\"top\" bgcolor=\"#564319\" style=\"background-color:#0c0d0d; font-family:Arial, Helvetica, sans-serif; padding:10px;\">\r\n                            <div align=center style=\"font-size:36px; color:#ffffff;\">\r\n                                <img src=\"D:\\Arthur_Clive\\netcore\\AuthorizedServer\\EmailTemplate\\logo-caption.png\" width=\"200px\" height=\"120px\" />\r\n                            </div>\r\n                        </td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td align=center style=\"background-color:#ffffff;padding:15px; font-family:Arial, Helvetica, sans-serif;\">\r\n                            <div style=\"font-size:20px;\">\r\n                                <span>\r\n                                    <b>Sample user</b>\r\n                                </span>\r\n                            </div>\r\n                        </td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td align=center style=\"background-color:#ffffff;padding:15px; font-family:Arial, Helvetica, sans-serif;\">\r\n                            <div style=\"font-size:20px;\">\r\n                                <b>Congratulations! You are registered...</b>\r\n                            </div>\r\n                        </td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td align=center style=\"background-color:#ffffff;padding:15px; font-family:Arial, Helvetica, sans-serif;\">\r\n                            <div style=\"font-size:20px;\">\r\n                                <b>Team Arthur Clive Welcomes you...</b>\r\n                            </div>\r\n                        </td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td align=center style=\"background-color:#ffffff;padding:15px; font-family:Arial, Helvetica, sans-serif;\">\r\n                            <div style=\"font-size:20px;\">\r\n                                <b>To verify your Arthur Clive account click on the link given below</b>\r\n                            </div>\r\n                        </td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td align=center style=\"background-color:#ffffff;padding:15px; font-family:Arial, Helvetica, sans-serif;\">\r\n                            <div style=\"font-size:20px;\">\r\n                                <span>\r\n                                    <b><a href ='https://artwear.in/'>Click Here</a></b>\r\n                                </span>\r\n                            </div>\r\n                        </td>\r\n                    </tr>\r\n                </table>\r\n            </td>\r\n        </tr>\r\n    </table>\r\n</body>\r\n\r\n</html>";

            //Act
            var result = EmailHelper.CreateEmailBody(fullname, link) as string;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEmail, result);
            Assert.IsNotNull(linkFromXML);
        }
    }

    [TestClass]
    public class GlobalHelper_IntegrationTest
    {
        [TestMethod]
        public void GlobalHelper_GetCurrentDir()
        {
            //Arrange

            //Act
            var result = GlobalHelper.GetCurrentDir() as string;

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GlobalHelper_ReadXML()
        {
            //Arrange

            //Act
            var result = GlobalHelper.ReadXML() as XElement;

            //Assert
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void GlobalHelper_GetIpConfig()
        {
            //Arrange

            //Act
            var result = GlobalHelper.GetIpConfig() as string;

            //Assert
            Assert.IsNotNull(result);
        }
    }

    [TestClass]
    public class MongoHelper_IntegrationTest
    {
        [TestMethod]
        public void MongoHelper_GetClient()
        {
            //Arrange
            var ipFromXML = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("ip").First().Value;
            var userFromXML = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("user").First().Value;
            var passwordFromXML = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("password").First().Value;
            var dbFromXML = GlobalHelper.ReadXML().Elements("mongo").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("db").First().Value;

            //Act
            var result = MongoHelper.GetClient() as MongoClient;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(ipFromXML);
            Assert.IsNotNull(userFromXML);
            Assert.IsNotNull(passwordFromXML);
            Assert.IsNotNull(dbFromXML);
        }

        //Pending
        //[TestMethod]
        public void MongoHelper_GetSingleObject()
        {
            //Arrange

            //Act

            //Assert
        }

        //Pending
        //[TestMethod]
        public void MongoHelper_GetListOfObjects()
        {
            //Arrange

            //Act

            //Assert
        }

        //Pending
        //[TestMethod]
        public void MongoHelper_UpdateSingleObject()
        {
            //Arrange

            //Act

            //Assert
        }

        //Pending
        //[TestMethod]
        public void MongoHelper_CheckForDatas()
        {
            //Arrange

            //Act

            //Assert
        }

        //Pending
        //[TestMethod]
        public void MongoHelper_RecordLoginAttempts()
        {
            //Arrange

            //Act

            //Assert
        }
    }

    [TestClass]
    public class SMSHelper_IntegrationTest
    {
        [TestMethod]
        public void SMSHlper_GetCredentials()
        {
            //Arrange
            var key1 = "accesskey";
            var key2 = "secretkey";

            //Act
            var accessKey = SMSHelper.GetCredentials(key1) as string;
            var secretKey = SMSHelper.GetCredentials(key2) as string;

            //Assert
            Assert.IsNotNull(accessKey);
            Assert.IsNotNull(secretKey);
        }

        //Pending
        //[TestMethod]
        public void SMSHlper_SendSMS()
        {
            //Arrange

            //Act

            //Assert
        }
    }

}
