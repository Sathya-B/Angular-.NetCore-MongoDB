using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AuthorizedServer.Models
{
    public class RegisterModel
    {   
        public ObjectId _id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string VerificationCode { get; set; }
        public string Status { get; set; }
        public DateTime OTPExp { get; set; }
    }
    public class SmsVerificationModel
    {
        public string PhoneNumber { get; set; }
        public string VerificationCode { get; set; }
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
