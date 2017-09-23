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
        public string GrantType { get; set; }
        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class RToken
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public int IsStop { get; set; }
    }
}
