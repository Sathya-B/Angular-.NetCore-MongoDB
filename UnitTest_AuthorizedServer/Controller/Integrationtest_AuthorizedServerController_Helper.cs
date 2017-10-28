using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using AuthorizedServer;
using AuthorizedServer.Controllers;
using AuthorizedServer.Repositories;
using Microsoft.Extensions.Options;

namespace UnitTest_AuthorizedServer.Controller
{
    class Integrationtest_AuthorizedServerController_Helper
    {
        public static IOptions<Audience> _settings;
        public static IRTokenRepository _repo;

        public Integrationtest_AuthorizedServerController_Helper(IOptions<Audience> settings, IRTokenRepository repo)
        {
            _settings = settings;
            _repo = repo;
        }

        public static AuthController GetAuthController()
        {
            AuthController authController = new AuthController(_settings,_repo);
            return authController;
        }

        public static ResponceData DeserializedResponceData(dynamic data)
        {
            ActionResultModel deserializedResponce = new ActionResultModel();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedResponce.GetType());
            deserializedResponce = ser.ReadObject(ms) as ActionResultModel;
            ms.Close();
            return deserializedResponce.Value;
        }
    }
}
