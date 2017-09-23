using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthorizedServer.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace AuthorizedServer.Helper
{
    public class AuthHelper
    {
        //scenario 1 ： get the access-token by username and password
        public ResponseData DoPassword(Parameters parameters, IRTokenRepository _repo, IOptions<Audience> _settings)
        {

            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            var rToken = new RToken
            {
                ClientId = parameters.UserName,
                RefreshToken = refresh_token,
                Id = Guid.NewGuid().ToString(),
                IsStop = 0
            };

            //store the refresh_token 
            if (_repo.AddToken(rToken).Result)
            {
                dynamic UserInfo = new System.Dynamic.ExpandoObject();
                UserInfo.FirstName = parameters.FullName;
                return new ResponseData
                {
                    Code = "999",
                    Message = "OK",
                    Content = UserInfo,
                    Data = GetJwt(parameters.UserName, refresh_token, _settings)
                };
            }
            else
            {
                return new ResponseData
                {
                    Code = "909",
                    Message = "can not add token to database",
                    Data = null
                };
            }
        }

          //scenario 2 ： get the access_token by refresh_token
        public ResponseData DoRefreshToken(Parameters parameters, IRTokenRepository _repo, IOptions<Audience> _settings)
        {
            var token = _repo.GetToken(parameters.RefreshToken, parameters.ClientId).Result;

            if (token == null)
            {
                return new ResponseData
                {
                    Code = "905",
                    Message = "can not refresh token",
                    Data = null
                };
            }

            if (token.IsStop == 1)
            {
                return new ResponseData
                {
                    Code = "906",
                    Message = "refresh token has expired",
                    Data = null
                };
            }

            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            token.IsStop = 1;
            //expire the old refresh_token and add a new refresh_token
            var updateFlag = _repo.ExpireToken(token).Result;

            var addFlag = _repo.AddToken(new RToken
            {
                ClientId = parameters.ClientId,
                RefreshToken = refresh_token,
                Id = Guid.NewGuid().ToString(),
                IsStop = 0
            });

            if (updateFlag && addFlag.Result)
            {
                return new ResponseData
                {
                    Code = "999",
                    Message = "OK",
                    Data = GetJwt(parameters.ClientId, refresh_token, _settings)
                };
            }
            else
            {
                return new ResponseData
                {
                    Code = "910",
                    Message = "can not expire token or a new token",
                    Data = null
                };
            }
        }

        public string GetJwt(string client_id, string refresh_token, IOptions<Audience> _settings)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, client_id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64)
            };

            var symmetricKeyAsBase64 = _settings.Value.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var jwt = new JwtSecurityToken(
                issuer: _settings.Value.Iss,
                audience: _settings.Value.Aud,
                claims: claims,
                notBefore: now,
                expires: now.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)TimeSpan.FromMinutes(2).TotalSeconds,
                refresh_token = refresh_token,
            };

            return JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }
    }
}
