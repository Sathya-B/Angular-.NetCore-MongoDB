
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Arthur_Clive.Data
{
    /// <summary>Details of role</summary>
    public class Roles
    {
        /// <summary>Object id given by mongodb</summary>
        public ObjectId _id { get; set; }
        /// <summary>Id of user role</summary>
        [Required]
        public int RoleID { get; set; }
        /// <summary>Name of role</summary>
        [Required]
        public string RoleName { get; set; }
        /// <summary>Level of user access</summary>
        [Required]
        public List<string> LevelOfAccess { get; set; }
    }

    /// <summary>Contails responce data</summary>
    public class ResponseData
    {
        /// <summary>Responce code for the responce</summary>
        [Required]
        public string Code { get; set; }
        /// <summary>Message in the responce</summary>
        public string Message { get; set; }
        /// <summary>Data in the responce</summary>
        public object Data { get; set; }
        /// <summary>Other contents of responce</summary>
        public object Content { get; set; }
    }

    /// <summary>Details of coupon</summary>
    public class Coupon
    {
        /// <summary></summary>
        public ObjectId Id { get; set; } 
        /// <summary>Coupon code</summary>
        [Required]
        public string Code { get; set; }
        /// <summary>For whom is the coupon applicable for</summary>
        [Required]
        public string ApplicableFor { get; set; }
        /// <summary>Expiry time of the coupon</summary>
        [Required]
        public DateTime ExpiryTime { get; set; }
        /// <summary>Coupon usage count</summary>
        [Required]
        public int UsageCount { get; set; }
        /// <summary>Value of coupon in percentage or amount</summary>
        [Required]
        public double Value { get; set; }
        /// <summary>If the value of coupon is persentage pass the flag as true</summary>
        [Required]
        public bool? Percentage { get; set; }
    }

    /// <summary>Contains registration details of the user </summary>
    public class RegisterModel
    {
        /// <summary>ObjectId given by MongoDB</summary>
        public ObjectId _id { get; set; }
        /// <summary>Titile of the user</summary>
        [Required]
        public string Title { get; set; }
        /// <summary>Fullname of user</summary>
        [Required]
        public string FullName { get; set; }
        /// <summary>Username of user</summary>
        public string UserName { get; set; }
        /// <summary>Role for user</summary>
        public string UserRole { get; set; }
        /// <summary>Dialcode for phonenumber of the user</summary>
        [Required]
        public string DialCode { get; set; }
        /// <summary>Phonenumber of the user</summary>
        [Required]
        public string PhoneNumber { get; set; }
        /// <summary>Email of the user</summary>
        [Required]
        public string Email { get; set; }
        /// <summary>SocialId of user incase the user login using facebook and gmail</summary>
        [Required]
        public string SocialId { get; set; }
        /// <summary>Password of the user</summary>
        [Required]
        public string Password { get; set; }
        /// <summary>Location of the user</summary>
        [Required]
        public string UserLocation { get; set; }
        /// <summary>Verification code sent ot the user</summary>
        public string VerificationCode { get; set; }
        /// <summary>Status of the user</summary>
        public string Status { get; set; }
        /// <summary>Count of invalid login attempts</summary>
        public int WrongAttemptCount { get; set; }
        /// <summary>OTP expiration time</summary>
        public DateTime OTPExp { get; set; }
    }
}
