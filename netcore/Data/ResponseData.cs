
using System.Text;

namespace Arthur_Clive.Data
{
    public class ResponseData
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public StringBuilder Form { get; set; }
    }
}
