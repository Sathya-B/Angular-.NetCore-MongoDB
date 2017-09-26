using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace AuthorizedServer.Helper
{
    public class EmailHelper
    {
        public static string GetCredentials(string key)
        {
            var result = GlobalHelper.ReadXML().Elements("amazonses").Where(x => x.Element("current").Value.Equals("test")).Descendants(key);
            return result.First().Value;
        }

        public static async Task<string> SendEmail(string fullname,string emailReceiver, string link)
        {
            string emailSender = GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsender").First().Value;
            using (var client = new AmazonSimpleEmailServiceClient(GetCredentials("accesskey"), GetCredentials("secretkey"), Amazon.RegionEndpoint.USWest2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = emailSender,
                    Destination = new Destination { ToAddresses = new List<string> { emailReceiver } },
                    Message = new Message
                    {
                        Subject = new Content(GlobalHelper.ReadXML().Elements("email").Where(x => x.Element("current").Value.Equals("Yes")).Descendants("emailsubject2").First().Value),
                        Body = new Body
                        {
                            Html = new Content(CreateEmailBody(fullname, "<a href ='" + link + "'>Click Here To Verify</a>"))
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

        public static string CreateEmailBody(string fullname, string link)
        {
            string emailBody;
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(dir, "EmailVerification.html");
            using (StreamReader reader = File.OpenText(path))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{FullName}", fullname);
            emailBody = emailBody.Replace("{Link}", link);
            return emailBody;
        }
    }
}
