using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WxAPI.Helper;
using WxAPI.Model;
using WxAPI.Services.IServices;
using WxAPI.Services.Services;

namespace WxAPI
{
    public class Startup
    {
        private readonly string AllowSpecificOrigin = "AllowSpecificOrigin";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new LogHelper());
            var basePath = AppContext.BaseDirectory;
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WxAPI", Version = "v1" });
                //开启加权小锁
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                // 在header中添加token，传递到后台
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                // 启用swagger验证功能,必须是 oauth2
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });

                var xmlPath = Path.Combine(basePath, "WxAPI.xml");//这个就是刚刚配置的xml文件名
                c.IncludeXmlComments(xmlPath);
                c.IncludeXmlComments(xmlPath, true); //默认的第二个参数是false，这个是controller的注释，记得修改

            });
            //读取appsettings.json配置信息
            services.Configure<TokenManagementModel>(Configuration.GetSection("tokenManagement"));
            services.Configure<WeChatModel>(Configuration.GetSection("weChat"));
            var token = Configuration.GetSection("tokenManagement").Get<TokenManagementModel>();

            //注入jwt验证服务，并在中间件管道中启用authentication中间件。//添加jwt验证：
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(token.Secret)),
                    ValidIssuer = token.Issuer,
                    ValidAudience = token.Audience,
                    ValidateIssuer = true, //是否验证issuer /颁发者
                    ValidateAudience = true, //是否验证 audience ////接收者
                    ValidateLifetime = true,//验证生命周期
                    RequireExpirationTime = true, //是否验证失效时间,是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ClockSkew = TimeSpan.Zero  //偏移量
                };
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        string response = "";
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                            response = JsonConvert.SerializeObject(new JsonReturn() { Status = (int)HttpStatusCode.BadRequest, Message = "token已经过期" });
                            LogHelper.Error(response);
                        }

                        context.Response.WriteAsync(response);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });



            //.AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });
            services.AddControllers(option =>
            {
                option.Filters.Add<AuthFilter>();
            });
            services.AddControllersWithViews();

            #region 跨域
            services.AddCors(options =>
            {
                options.AddPolicy(AllowSpecificOrigin,
                    builder =>
                    {
                        builder.AllowAnyMethod()
                            .AllowAnyOrigin()
                            .AllowAnyHeader();
                    });
            });
            #endregion

            //注册接口服务
            services.AddScoped<IAuthenticationServies, AuthenticationServies>();
            services.AddScoped<IQuestionFeedbackServies, QuestionFeedbackServies>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WxAPI v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            //加入Jwt鉴权添加在app.UseRouting()和app.UseAuthorization()之间
            app.UseAuthentication(); //开启认证
            app.UseAuthorization();//授权

            //CORS 中间件必须配置为在对 UseRouting 和 UseEndpoints的调用之间执行。 配置不正确将导致中间件停止正常运行。
            app.UseCors(AllowSpecificOrigin);

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
