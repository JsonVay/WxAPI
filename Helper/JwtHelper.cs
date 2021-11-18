using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WxAPI.Model;

namespace WxAPI.Helper
{
    public  class JwtHelper
    {
        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="_tokenManagement"></param>
        /// <returns></returns>
        public static dynamic GetAuthToken(string openid, TokenManagementModel _tokenManagement)
        {
            DateTime now = DateTime.Now;
            var expires = now.AddMinutes(_tokenManagement.AccessExpiration);
            var claims = new[]
            {
                new Claim("openid",openid),
                new Claim("expiration", expires.ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenManagement.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(
                    //颁发者
                    issuer: _tokenManagement.Issuer,
                    //接收者
                    audience: _tokenManagement.Audience,
                    //自定义参数
                    claims: claims,
                    //生效时间
                    notBefore: now,
                     //过期时间
                    expires: expires,
                    //签名证书
                    signingCredentials: credentials
                 );
            var responseJson = new
            {
                Status = (int)HttpStatusCode.OK,
                Message = "请求成功",
                Data = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Expires = expires.ToString(),
                Openid = openid
            };
            return responseJson;
            
        }

        /// <summary>
        /// 解析Token
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static JwtSecurityToken SerializeToken(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);
            return jwtToken;
        }
    }
}
