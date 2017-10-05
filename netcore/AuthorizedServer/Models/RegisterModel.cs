using System;
using MongoDB.Bson;

namespace AuthorizedServer.Models
{
    public class RegisterModel
    {
        public ObjectId _id { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string DialCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserLocation { get; set; }
        public string VerificationCode { get; set; }
        public string Status { get; set; }
        public int WrongAttemptCount { get; set; }
        public DateTime OTPExp { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string UserName { get; set; }
        public string UserLocation { get; set; }
        public string VerificationCode { get; set; }
        public string Status { get; set; }
        public DateTime OTPExp { get; set; }
    }

    public class VerificationModel
    {
        public string UserName { get; set; }
        public string VerificationCode { get; set; }
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordModel
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class SocialLoginModel
    {   
        public string Token { get; set; }
        public string Email { get; set; }
        public string ID { get; set; }
    }
    
    public class GoogleVerificationModel
    {
        public string name { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string azp { get; set; }
        public string aud { get; set; }
        public string sub { get; set; }
        public string at_hash { get; set; }
        public string iss { get; set; }
        public string jti { get; set; }
        public string iat { get; set; }
        public string exp { get; set; }
        public string picture { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string locale { get; set; }
        public string alg { get; set; }
        public string kid { get; set; }
    }

    public class FacebookVerificationModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
