using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Arthur_Clive.Logger;

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
        public static async Task<string> SendEmail(string fullname, string emailReceiver, string message)
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
                    LoggerDataAccess.CreateLog("EmailHelper", "SendEmail", "SendEmail", ex.Message);
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
            var path = Path.Combine(dir, "PublicPostEmailTemplate.html");
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
        public static async Task<string> SendEmailToAdmin(string userName,string productInfo,long orderQuantity,long productStock,long orderId)
        {
            string emailSender = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsender").First().Value;
            string emailReceiver = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailreceiver").First().Value;
            string link = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("websitelink").First().Value;
            string emailSubject = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsubject3").First().Value + "OrderId : " + orderId + "&" + "UserName : " + userName ;
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
                            Html = new Content(CreateEmailBody_ErrorReport(userName,productInfo,orderQuantity,productStock,orderId.ToString()))
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
        public static string CreateEmailBody_ErrorReport(string userName, string productInfo, long orderQuantity,long productStock,string orderId)
        {
            string emailBody;
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "EmailTemplate\\ErrorReport.html");
            using (StreamReader reader = File.OpenText(path))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{OrderId}",orderId);
            emailBody = emailBody.Replace("{UserName}", userName);
            emailBody = emailBody.Replace("{ProductInfo}", productInfo);
            emailBody = emailBody.Replace("{OrderQuantity}", orderQuantity.ToString());
            emailBody = emailBody.Replace("{ProductStock}", productStock.ToString());
            return emailBody;
        }
    }
}
