using MongoDB.Bson.Serialization.Attributes;

namespace AuthorizedServer
{
    public class ResponseData
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Content { get; set; }
    }

    public class Audience
    {
        public string Secret { get; set; }
        public string Iss { get; set; }
        public string Aud { get; set; }
    }

    public class Parameters
    {
        public string grant_type { get; set; }
        public string refresh_token { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string fullname { get; set; }
    }

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
