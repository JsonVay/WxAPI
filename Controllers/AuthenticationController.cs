using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WxAPI.Model;
using WxAPI.Services.IServices;

namespace WxAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationServies _authenticationServies;
        public AuthenticationController(IAuthenticationServies authenticationServies)
        {
            _authenticationServies = authenticationServies;
        }
        
        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> RequestToken(string code)
        {
            var token =await  _authenticationServies.GetToken(code);
            return Ok(token);
        }

        /// <summary>
        /// 刷新token
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> RefreshToken(string openid)
        {
            var token = await _authenticationServies.RefreshToken(openid);
            return Ok(token);
        }
    }
}
