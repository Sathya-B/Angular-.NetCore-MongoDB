
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Arthur_Clive.Data
{
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
}
