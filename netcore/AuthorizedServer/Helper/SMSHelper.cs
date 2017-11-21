using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AuthorizedServer.Logger;
using MongoDB.Driver;
using MH = AuthorizedServer.Helper.MongoHelper;

namespace AuthorizedServer.Helper
{
    /// <summary>Helper for Amazon SNS service</summary>
    public class SMSHelper
    {
        /// <summary>Client for MongoDB</summary>
        public MongoClient _client;
        /// <summary></summary>
        public static IMongoDatabase logger_db;
        /// <summary></summary>
        public static IMongoCollection<ApplicationLogger> serverlogCollection;

        /// <summary></summary>
        public SMSHelper()
        {
            _client = MH.GetClient();
            logger_db = _client.GetDatabase("ArthurCliveLogDB");
            serverlogCollection = logger_db.GetCollection<ApplicationLogger>("ServerLog");
        }

        /// <summary>Get amazon SNS service credentials from xml file</summary>
        /// <param name="key"></param>
        public static string GetCredentials(string key)
        {
            var result = GlobalHelper.ReadXML().Elements("amazonsns").Where(x => x.Element("current").Value.Equals("Yes")).Descendants(key);
            return result.First().Value;
        }

        /// <summary>Send sms using amazon SNS service</summary>
        /// <param name="phoneNumber"></param>
        /// <param name="otp"></param>
        public static string SendSMS(string phoneNumber, string otp)
        {
            try
            {
                AmazonSimpleNotificationServiceClient smsClient = new AmazonSimpleNotificationServiceClient
                    (GetCredentials("accesskey"), GetCredentials("secretkey"), Amazon.RegionEndpoint.APSoutheast1);

                var smsAttributes = new Dictionary<string, MessageAttributeValue>();

                MessageAttributeValue senderID = new MessageAttributeValue();
                senderID.DataType = "String";
                senderID.StringValue = "ArthurClive";

                MessageAttributeValue sMSType = new MessageAttributeValue();
                sMSType.DataType = "String";
                sMSType.StringValue = "Transactional";

                MessageAttributeValue maxPrice = new MessageAttributeValue();
                maxPrice.DataType = "Number";
                maxPrice.StringValue = "0.5";

                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                smsAttributes.Add("AWS.SNS.SMS.SenderID", senderID);
                smsAttributes.Add("AWS.SNS.SMS.SMSType", sMSType);
                smsAttributes.Add("AWS.SNS.SMS.MaxPrice", maxPrice);

                string message = "Verification code for your Arthur Clive registration request is " + otp;

                PublishRequest publishRequest = new PublishRequest();
                publishRequest.Message = message;
                publishRequest.MessageAttributes = smsAttributes;
                publishRequest.PhoneNumber = "+91" + phoneNumber;

                Task<PublishResponse> result = smsClient.PublishAsync(publishRequest, token);
                return "Success";
            }
            catch (Exception ex)
            {
                LoggerDataAccess.CreateLog("SMSHelper", "SendSMS", ex.Message, serverlogCollection);
                return "Failed";
            }
        }
    }
}
