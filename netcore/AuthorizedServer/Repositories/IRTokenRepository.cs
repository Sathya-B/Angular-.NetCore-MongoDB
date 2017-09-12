using System.Threading.Tasks;

namespace AuthorizedServer.Repositories
{
    public interface IRTokenRepository
    {
        Task<bool> AddToken(RToken token);

        Task<bool> ExpireToken(RToken token);

        Task<RToken> GetToken(string refresh_token,string client_id);
    }
}
