using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthorizedServer
{
    public class RToken
    {
        public string Id { get; set; }

        [BsonElement("client_id")]
        public string ClientId { get; set; }

        [BsonElement("refresh_token")]
        public string RefreshToken { get; set; }
        
        [BsonElement("isstop")]
        public int IsStop { get; set; }
    }    
}
