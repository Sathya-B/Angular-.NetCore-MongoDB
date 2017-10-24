using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AuthorizedServer.Models
{
    public class UserInfo
    {
        /// <summary>ObjectId given by MongoDB</summary>
        public ObjectId _id { get; set; }
        /// <summary>Fullname of user</summary>
        public string FullName { get; set; }
        /// <summary>Username of user</summary>
        public string UserName { get; set; }
        /// <summary>Role for user</summary>
        public string DialCode { get; set; }
        /// <summary>Phonenumber of the user</summary>
        public string PhoneNumber { get; set; }
        /// <summary>Email of the user</summary>
        public string Email { get; set; }
    }
}
