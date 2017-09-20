using System;
using System.Threading.Tasks;
using AuthorizedServer.Logger;

namespace AuthorizedServer.Helper
{
    public class EmailHelper
    {
        public static async Task<string> SendEmail(string toEmail,string otp)
        {
            try
            {
                String username = "AKIAIPK63CQGCQD4BTJQ";  
                String password = "Ai3QwhoWSd9OnRtKKD9kjG3Z7+dKjrwcJyBSYfHakYDp";  
                String host = "email-smtp.us-east.amazonaws.com";
                int port = 25;

                using (var client = new System.Net.Mail.SmtpClient(host, port))
                {
                    client.Credentials = new System.Net.NetworkCredential(username, password);
                    client.EnableSsl = true;

                    client.Send
                    (
                              toEmail,  
                              toEmail,    
                              "Arthur Clive Account Verification",
                              "Click the provided link to verify your account"
                    );
                }
                return "Success";
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("EmailHelper", "SendEmail", "SendEmail", ex.Message);
                return "Failed";
            }
        }
    }
}
