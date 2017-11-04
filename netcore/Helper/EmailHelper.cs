using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Arthur_Clive.Data;
using Arthur_Clive.Logger;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Arthur_Clive.Helper
{
    /// <summary>Helper method for Amazon SES service</summary>
    public class EmailHelper
    {
        /// <summary>Get Amazon SES credentials from xml file</summary>
        /// <param name="key"></param>
        public static string GetCredentials(string key)
        {
            var xx = GlobalHelper.ReadXML();
            var result = GlobalHelper.ReadXML().Elements("amazonses").Where(x => x.Element("current").Value.Equals("Yes")).Descendants(key);
            return result.First().Value;
        }

        /// <summary>Send email using Amazon SES service</summary>
        /// <param name="fullname"></param>
        /// <param name="emailReceiver"></param>
        /// <param name="message"></param>
        public static async Task<string> SendEmail_ToUsers(string fullname, string emailReceiver, string message)
        {
            string emailSender = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsender").First().Value;
            string link = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("websitelink").First().Value;
            using (var client = new AmazonSimpleEmailServiceClient(GetCredentials("accesskey"), GetCredentials("secretkey"), Amazon.RegionEndpoint.USWest2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = emailSender,
                    Destination = new Destination { ToAddresses = new List<string> { emailReceiver } },
                    Message = new Message
                    {
                        Subject = new Content(GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsubject1").First().Value),
                        Body = new Body
                        {
                            Html = new Content(CreateEmailBody(fullname, "<a href ='" + link + "'>Click Here</a>", message))
                        }
                    }
                };
                try
                {
                    var responce = await client.SendEmailAsync(sendRequest);
                    return "Success";
                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("EmailHelper", "SendEmail_ToUsers", "SendEmail_ToUsers", ex.Message);
                    return ex.Message;
                }
            }
        }

        /// <summary>Create email body       /// </summary>
        /// <param name="fullname"></param>
        /// <param name="link"></param>
        /// <param name="message"></param>
        public static string CreateEmailBody(string fullname, string link, string message)
        {
            string emailBody;
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "MessageFromAdmin.html");
            using (StreamReader reader = File.OpenText(path))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{FullName}", fullname);
            emailBody = emailBody.Replace("{Message}", message);
            emailBody = emailBody.Replace("{Link}", link);
            return emailBody;
        }

        /// <summary>Send email to admin is the orders product quantity is higher than the product stock</summary>
        public static async Task<string> SendEmailToAdmin(string userName, string email, string productInfo, long orderQuantity, long productStock, long orderId)
        {
            string emailSender = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsender").First().Value;
            string emailReceiver = email;
            string link = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("websitelink").First().Value;
            string emailSubject = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsubject3").First().Value + "OrderId : " + orderId + "&" + "UserName : " + userName;
            using (var client = new AmazonSimpleEmailServiceClient(GetCredentials("accesskey"), GetCredentials("secretkey"), Amazon.RegionEndpoint.USWest2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = emailSender,
                    Destination = new Destination { ToAddresses = new List<string> { emailReceiver } },
                    Message = new Message
                    {
                        Subject = new Content(emailSubject),
                        Body = new Body
                        {
                            Html = new Content(CreateEmailBody_ErrorReport(userName, productInfo, orderQuantity, productStock, orderId.ToString()))
                        }
                    }
                };
                try
                {
                    var responce = await client.SendEmailAsync(sendRequest);
                    return "Success";
                }
                catch (Exception ex)
                {
                    LoggerDataAccess.CreateLog("EmailHelper", "SendEmail", "SendEmail", ex.Message);
                    return ex.Message;
                }
            }
        }

        /// <summary>Create email body to be sent to admin reporting a problem</summary>
        /// <param name="userName"></param>
        /// <param name="productInfo"></param>
        /// <param name="orderQuantity"></param>
        /// <param name="productStock"></param>
        /// <param name="orderId"></param>
        public static string CreateEmailBody_ErrorReport(string userName, string productInfo, long orderQuantity, long productStock, string orderId)
        {
            string emailBody;
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "EmailTemplate\\ErrorReport.html");
            using (StreamReader reader = File.OpenText(path))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{OrderId}", orderId);
            emailBody = emailBody.Replace("{UserName}", userName);
            emailBody = emailBody.Replace("{ProductInfo}", productInfo);
            emailBody = emailBody.Replace("{OrderQuantity}", orderQuantity.ToString());
            emailBody = emailBody.Replace("{ProductStock}", productStock.ToString());
            return emailBody;
        }

        /// <summary>Send gift by email</summary>
        /// <param name="orderId">Id of order</param>
        /// <param name="productInfo">Info of product</param>
        public static async Task<string> SendGift(long orderId, string productInfo)
        {
            try
            {
                var productArray = productInfo.Split(":");
                var checkOrder = MongoHelper.GetSingleObject(Builders<BsonDocument>.Filter.Eq("OrderId", orderId), "OrderDB", "OrderInfo").Result;
                if (checkOrder != null)
                {
                    var orderInfo = BsonSerializer.Deserialize<OrderInfo>(checkOrder);
                    foreach (var product in productArray)
                    {
                        if (product.Contains("Gifts"))
                        {
                            foreach (var info in orderInfo.ProductDetails)
                            {
                                if (product == info.ProductSKU)
                                {
                                    Random generator = new Random();
                                    var couponCode =  "CU" + generator.Next(0, 1000000).ToString("D6");
                                    Coupon coupon = new Coupon
                                    {
                                        Code = couponCode,
                                        ApplicableFor = "All",
                                        UsageCount = 1,
                                        Percentage = false,
                                        Value = info.ProductInCart.ProductPrice,
                                        ExpiryTime = DateTime.UtcNow.AddYears(10)
                                    };
                                    //Insert coupon to db
                                    await MongoHelper._client.GetDatabase("CouponDB").GetCollection<Coupon>("Coupon").InsertOneAsync(coupon);
                                    var user = MongoHelper.GetSingleObject(Builders<BsonDocument>.Filter.Eq("UserName", orderInfo.UserName), "Authentication", "Authentication").Result;
                                    if (user == null)
                                    {
                                        return "User not found";
                                    }
                                    var userData = BsonSerializer.Deserialize<RegisterModel>(user);
                                    var imageUrl = "https://s3.ap-south-1.amazonaws.com/arthurclive-products/" + product + ".jpg"; 
                                    var imageLink = "<img src=" + imageUrl + " width=\"300px\" heigh=\"900px\" alt=" + product + ">";
                                    string emailSender = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsender").First().Value;
                                    using (var client = new AmazonSimpleEmailServiceClient(GetCredentials("accesskey"), GetCredentials("secretkey"), Amazon.RegionEndpoint.USWest2))
                                    {
                                        var sendRequest = new SendEmailRequest
                                        {
                                            Source = emailSender,
                                            Destination = new Destination { ToAddresses = new List<string> { info.ProductInCart.ProductFor } },
                                            Message = new Message
                                            {
                                                Subject = new Content(GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsubject4").First().Value),
                                                Body = new Body
                                                {
                                                    Html = new Content(CreateEmailBody_SendGiftCard(info.ProductInCart.ProductPrice.ToString(),couponCode, info.ProductInCart.ProductDescription, userData.FullName,imageLink))
                                                }
                                            }
                                        };
                                        var responce = await client.SendEmailAsync(sendRequest);
                                    }
                                }
                            }
                        }
                    }
                    return "Success";
                }
                else
                {
                    return "Order info not found";
                }
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("EmailHelper", "SendGiftCard", "SendGiftCard", ex.Message);
                return "Failed";
            }
        }

        /// <summary>Create email body to send gift through email</summary>
        /// <param name="value"></param>
        /// <param name="couponCode"></param>
        /// <param name="message"></param>
        /// <param name="fullName"></param>
        /// <param name="image"></param>
        public static string CreateEmailBody_SendGiftCard(string value, string couponCode, string message, string fullName,string image)
        {
            string emailBody;
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "EmailTemplate\\SendGift.html");
            using (StreamReader reader = File.OpenText(path))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{GiftValue}", value);
            emailBody = emailBody.Replace("{CouponCode}", couponCode);
            emailBody = emailBody.Replace("{Message}", message);
            emailBody = emailBody.Replace("{FullName}", fullName);
            emailBody = emailBody.Replace("{Image}", image);
            return emailBody;
        }
    }
}
