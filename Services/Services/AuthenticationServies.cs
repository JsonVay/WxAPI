using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WxAPI.Helper;
using WxAPI.Model;
using WxAPI.Services.IServices;
using Dapper;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;

namespace WxAPI.Services.Services
{
    public class AuthenticationServies : IAuthenticationServies
    {
        private readonly TokenManagementModel _tokenManagement;
        private readonly WeChatModel _weChatModel;
        public AuthenticationServies(IOptions<TokenManagementModel> tokenManagement, IOptions<WeChatModel> weChatModel)
        {
            _tokenManagement = tokenManagement.Value;
            _weChatModel = weChatModel.Value;
        }

        public async Task<dynamic> GetToken(string code)
        {
            //string openid = await GetOpenIdByCode(code);
            //if (string.IsNullOrEmpty(openid)) throw new Exception("openid not null");
            return JwtHelper.GetAuthToken("o9DD15atPIEBYfHKWU3MQbsWvP9s", _tokenManagement);
        }

        public async Task<dynamic> RefreshToken(string openid)
        {
           return await JwtHelper.GetAuthToken(openid, _tokenManagement);
        }
        public bool VerificationToken(string jwtStr)
        {
            var jwtSecurity = JwtHelper.SerializeToken(jwtStr);
            if(jwtSecurity!=null&& jwtSecurity.Claims != null)
            {
                var tt2 = jwtSecurity.Issuer;
                jwtSecurity.Claims.ToList().ForEach(x=> { 
                 if(x.Type.ToLower()== "openid")
                    {
                        if (string.IsNullOrWhiteSpace(x.Value)) throw new Exception("openid未传入");
                        var model = QueryOpenId(x.Value);
                        if(model==null||(model!=null&&string.IsNullOrWhiteSpace(model.OpenId)))
                            throw new Exception("openid不正确");
                    }
                });
            }
            return true;
        }
        
        public async Task<string> GetOpenIdByCode(string code)
        {
            var appid = _weChatModel.Appid;
            var secret = _weChatModel.Secret;
            if (string.IsNullOrEmpty(code)) throw new Exception("code is required");
            //获取openid
            var url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appid}&secret={secret}&js_code={code}&grant_type=authorization_code";
            var resp = await new HttpClient().GetStringAsync(url);
            if (string.IsNullOrEmpty(resp)) throw new Exception("request error");
            var weChatModel = Newtonsoft.Json.JsonConvert.DeserializeObject<WeChatModel>(resp);
            if (weChatModel != null && !string.IsNullOrWhiteSpace(weChatModel.OpenId))
            {
                var model = QueryOpenId(weChatModel.OpenId);
                if (model != null && !string.IsNullOrWhiteSpace(model.OpenId)) return model.OpenId;
                if(!InsertOpenId(weChatModel.OpenId, weChatModel.Session_key)) throw new Exception("OpenId未保存");
                return weChatModel.OpenId;
            }
            return "";
        }

        public WeChatModel QueryOpenId(string openId)
        {
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            var sql = $"select * from Wx_User where OpenId=@OpenId";
            return conn.Query<WeChatModel>(sql,new { OpenId = openId }).FirstOrDefault();
        }

        public bool InsertOpenId(string openId,string sessionkey)
        {
            var sql = $"insert into Wx_User(OpenId,Sessionkey) values(@OpenId,@Sessionkey)";
            using var conn = new SqlConnection(_weChatModel.SqlConnStrings);
            return conn.Execute(sql, new { OpenId = openId, Sessionkey = sessionkey }) > 0;
        }
    }
}
