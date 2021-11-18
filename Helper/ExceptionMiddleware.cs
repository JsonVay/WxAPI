using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WxAPI.Model;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace WxAPI.Helper
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private IWebHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            this._next = next;
            this._environment = environment;
        }
        //InvokeAsync更符合TAP编程模式，Invoke兼容
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                var features = context.Features;
            }
            catch (Exception e)
            {
                await HandleException(context, e);
            }
        }

        private async Task HandleException(HttpContext context, Exception e)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "text/json;charset=utf-8;";

            string error = "";

            void ReadException(Exception ex)
            {
                error += string.Format("{0} | {1} | {2}", ex.Message, ex.StackTrace, ex.InnerException);
                if (ex.InnerException != null)
                {
                    ReadException(ex.InnerException);
                }
            }

            ReadException(e);
            LogHelper.Error(error);

            if (_environment.IsDevelopment())
            {
                var json = new JsonReturn() { Status = context.Response.StatusCode, Message = e.Message };
                error = JsonConvert.SerializeObject(json);
            }
            else
                error = JsonConvert.SerializeObject(new JsonReturn() { Status = context.Response.StatusCode, Message = "出错了" });
            await context.Response.WriteAsync(error);
        }
    }
}
