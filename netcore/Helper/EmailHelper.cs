using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace Arthur_Clive.Helper
{
    public class EmailHelper
    {
        public static string GetCredentials(string key)
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var xmlStr = File.ReadAllText(Path.Combine(dir, "AmazonKeys.xml"));
            var str = XElement.Parse(xmlStr);
            var result = str.Elements("amazonses").Where(x => x.Element("current").Value.Equals("raguvarthan.n@turingminds.com")).Descendants(key);
            return result.First().Value;
        }

        public static async Task<string> SendEmail(string fullname, string emailReceiver, string message)
        {
            string emailSender = "raguvarthan.n@turingminds.com";
            string link = "https://artwear.in/";
            using (var client = new AmazonSimpleEmailServiceClient(GetCredentials("accesskey"), GetCredentials("secretkey"), Amazon.RegionEndpoint.USWest2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = emailSender,
                    Destination = new Destination { ToAddresses = new List<string> { emailReceiver } },
                    Message = new Message
                    {
                        Subject = new Content("Message from the Arthur Clive admin."),
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
                    return ex.Message;
                }
            }
        }

        public static string CreateEmailBody(string fullname, string link, string message)
        {
            string emailBody;
            using (StreamReader reader = System.IO.File.OpenText("D:/Arthur_Clive/netcore/EmailTemplate/PublicPostEmailTemplate.html"))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{FullName}", fullname);
            emailBody = emailBody.Replace("{Message}", message);
            emailBody = emailBody.Replace("{Link}", link);
            return emailBody;
        }
    }
}
