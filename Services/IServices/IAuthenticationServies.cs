using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace WxAPI.Services.IServices
{
    public interface IAuthenticationServies
    {
       Task<dynamic> GetToken(string code);

        bool VerificationToken(string jwtStr);

        Task<dynamic> RefreshToken(string openid);
    }
}
