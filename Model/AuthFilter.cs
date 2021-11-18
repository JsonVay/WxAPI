using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WxAPI.Helper;
using WxAPI.Services.IServices;
using WxAPI.Services.Services;

namespace WxAPI.Model
{
    public class AuthFilter: IAuthorizationFilter
    {
        private readonly IAuthenticationServies _authenticationServies;
        public AuthFilter(IAuthenticationServies authenticationServies)
        {
            _authenticationServies = authenticationServies;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var descriptor = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor; 
            var allowanyone = descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(IAllowAnonymous), true).Any() || descriptor.MethodInfo.GetCustomAttributes(typeof(IAllowAnonymous), true).Any();
            if (allowanyone) return;
            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ","");
            if (token != null&&!string.IsNullOrWhiteSpace(token))
            {
                var result = _authenticationServies.VerificationToken(token);
                if (!result)
                {
                    throw new Exception("token验证不通过");
                }
            }
            else
            {
                throw new Exception("token不能为空");
            }
        }
}
 }

        