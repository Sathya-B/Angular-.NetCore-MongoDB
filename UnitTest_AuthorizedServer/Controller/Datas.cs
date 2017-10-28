
namespace UnitTest_AuthorizedServer.Controller
{
    public class ActionResultModel
    {
        public dynamic _t { get; set; }
        public ResponceData Value { get; set; }
        public dynamic Formatters { get; set; }
        public dynamic ContentTypes { get; set; }
        public dynamic DeclaredType { get; set; }
        public int StatusCode { get; set; }
    }

    public class ResponceData
    {
        public dynamic _t { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public dynamic Data { get; set; }
        public dynamic Content { get; set; }
    }
}
