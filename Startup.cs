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
                //������ȨС��
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                // ��header�����token�����ݵ���̨
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                // ����swagger��֤����,������ oauth2
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.ApiKey
                });

                var xmlPath = Path.Combine(basePath, "WxAPI.xml");//������Ǹո����õ�xml�ļ���
                c.IncludeXmlComments(xmlPath);
                c.IncludeXmlComments(xmlPath, true); //Ĭ�ϵĵڶ���������false�������controller��ע�ͣ��ǵ��޸�

            });
            //��ȡappsettings.json������Ϣ
            services.Configure<TokenManagementModel>(Configuration.GetSection("tokenManagement"));
            services.Configure<WeChatModel>(Configuration.GetSection("weChat"));
            var token = Configuration.GetSection("tokenManagement").Get<TokenManagementModel>();

            //ע��jwt��֤���񣬲����м���ܵ�������authentication�м����//���jwt��֤��
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
                    ValidateIssuer = true, //�Ƿ���֤issuer /�䷢��
                    ValidateAudience = true, //�Ƿ���֤ audience ////������
                    ValidateLifetime = true,//��֤��������
                    RequireExpirationTime = true, //�Ƿ���֤ʧЧʱ��,�Ƿ���֤Token��Ч�ڣ�ʹ�õ�ǰʱ����Token��Claims�е�NotBefore��Expires�Ա�
                    ClockSkew = TimeSpan.Zero  //ƫ����
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
                            response = JsonConvert.SerializeObject(new JsonReturn() { Status = (int)HttpStatusCode.BadRequest, Message = "token�Ѿ�����" });
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

            #region ����
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

            //ע��ӿڷ���
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

            //����Jwt��Ȩ�����app.UseRouting()��app.UseAuthorization()֮��
            app.UseAuthentication(); //������֤
            app.UseAuthorization();//��Ȩ

            //CORS �м����������Ϊ�ڶ� UseRouting �� UseEndpoints�ĵ���֮��ִ�С� ���ò���ȷ�������м��ֹͣ�������С�
            app.UseCors(AllowSpecificOrigin);

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
