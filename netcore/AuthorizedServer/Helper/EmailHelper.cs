using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace AuthorizedServer.Helper
{
    public class EmailHelper
    {
        public static async Task<string> SendEmail(string fullname, string emailReceiver, string otp)
        {
            string emailSender = "raguvarthan.n@turingminds.com";
            using (var client = new AmazonSimpleEmailServiceClient("AKIAIMRQIQV343SHCTXQ", "mqiQXSzZNwyH0q+krpgXuONLwFul81ssc4JaolSU", Amazon.RegionEndpoint.USWest2))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = emailSender,
                    Destination = new Destination { ToAddresses = new List<string> { emailReceiver } },
                    Message = new Message
                    {
                        Subject = new Content("Verification of your ArthurClive account."),
                        Body = new Body
                        {
                            Html = new Content(CreateEmailBody(fullname, emailReceiver, otp, "", "< a href =\"http://google.com\">Click Here</a>"))
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

        public static string CreateEmailBody(string fullname, string username, string otp, string message, string link)
        {
            string emailBody;
            using (StreamReader reader = System.IO.File.OpenText("D:/Arthur_Clive/netcore/AuthorizedServer/EmailTemplate/EmailVerification.html"))
            {
                emailBody = reader.ReadToEnd();
            }
            emailBody = emailBody.Replace("{FullName}", fullname);
            emailBody = emailBody.Replace("{UserName}", username);
            emailBody = emailBody.Replace("{OTP}", otp);
            emailBody = emailBody.Replace("{Message}", message);
            emailBody = emailBody.Replace("{Link}", link);
            return emailBody;
        }
    }
}
