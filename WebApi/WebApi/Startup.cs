using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Repository;
using System.IO;
using System.Text;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // EF
            var connection = Configuration.GetConnectionString("MysqlConnection");
            //services.AddDbContext<MysqlDbContext>(options => options.UseMySql(connection));
            services.AddDbContext<WriteDbContext>(options =>
            {
                options.UseMySql(connection);
            }
            );

            services.AddDbContext<ReadDbContext>(options =>
            {
                options.UseMySql(connection);
            }
            );

            services.AddControllers(option =>
            {
                //�����������ʽӿڱ����ߴ����� ȫ�ֽӿڶ���ҪȨ����֤ �������AllowAnonymous
                option.Filters.Add<AuthExtension>();
                //option.Filters.Add<AuthAttribute>();
            }).AddNewtonsoftJson(options =>
            {
                //ȫ�����ڸ�ʽ��
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //���ص�Json��Сдԭ�����
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //ȡ������Josn����Ĭ������ĸСд
                options.UseMemberCasing();
            });

            //��ʽһȫ��ʵ�ֽӿڹ���ApiExplorerSettingsAttribute �������ӿ��ϲ�����д
            //services.AddControllers(options =>
            //{
            //    options.Conventions.Add(new GroupNameActionModelConvention());
            //});
            //��ʽ��ȫ��ʵ�ֽӿڹ���ApiExplorerSettingsAttribute �������ӿ��ϲ�����д
            services.AddControllers(options =>
            {
                options.Conventions.Add(new GroupNameActionControllerConvention());
            });


            //ע��JWT��֤��������  AddAuthenticationע�������֤���� ����Ϊ��֤����
            //AddJwtBearer  ����jwt��֤  ����Ϊί��
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                //��ȡ��Կ
                var secretByte = Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //��֤token������  ֻ�к��Issuer������Token�ű�����
                    ValidateIssuer = true,
                    ValidIssuer = Const.Domain,// Configuration["Authentication:Issuer"],
                    //token������
                    ValidateAudience = true,
                    ValidAudience = Const.Domain,// Configuration["Authentication:Audience"],
                    //��֤token�Ƿ����
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //����˽Կ������
                    IssuerSigningKey = new SymmetricSecurityKey(secretByte)

                };
            });
            // services.AddScoped<IAuthorizationHandler, AuthExtension>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            ////���ýӿ�ͳһ���ع�����
            services.AddMvc(options =>
            {
                // options.Filters.Add(typeof(ActionFilterTool));

            }).AddWebApiConventions();//��Ӵ�����HttpResponseMessage ����������������

            //ע��swagger����
            #region Knife4Jui
            services.Configure<Knife4UIOptions>(options =>
            {
                Configuration.Bind("Knife4UI", options);
            });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "�ӿ��ĵ�",
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Name = "Ϫ��",
                        Email = "971350106@qq.com",
                        Url = null
                    }
                });

                options.CustomOperationIds(apiDesc =>
                {
                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    return controllerAction.ControllerName + "-" + controllerAction.ActionName;
                });

                options.OrderActionsBy(apiDescription => apiDescription.RelativePath.Length.ToString());

                //�ӿڵ�ַ�ɶ�ѡ
                options.AddServer(new OpenApiServer() { Url = "http://localhost:8085", Description = "��ַ1" });
                options.AddServer(new OpenApiServer() { Url = "http://127.0.0.1:5001", Description = "��ַ2" });
                //192.168.28.213���ұ���IP
                options.AddServer(new OpenApiServer() { Url = "http://192.168.28.213:5002", Description = "��ַ3" });

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebApi.xml");
                options.IncludeXmlComments(filePath, true);

                //��swaggerȫ�ֽӿ����JwtBearer��֤
                //����JwtBearer��֤��ʽһ Authorization
                options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme()
                {
                    Description = "���Ƿ�ʽһ(ֱ�����������������֤��Ϣ������Ҫ�ڿ�ͷ���Bearer)",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                //    //����JwtBearer��֤��ʽ��
                //    //options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme()
                //    //{
                //    //    Description = "���Ƿ�ʽ��(JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�)",
                //    //    Name = "Authorization",//jwtĬ�ϵĲ�������
                //    //    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                //    //    Type = SecuritySchemeType.ApiKey
                //    //});

                //����һ��Scheme��ע�������IdҪ������AddSecurityDefinition�еĲ���nameһ��
                var scheme = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "JwtBearer" }
                };
                //ע��ȫ����֤�����еĽӿڶ�����ʹ����֤��
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    [scheme] = new string[0]
                });

            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //Ԥ�����ж� ������debug
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //�����ض�����м��
            //app.UseHttpsRedirection();
            app.UseRouting();//·�ɣ�������

            //���jwt��֤ �˹ܵ�ִ�����Ⱥ�˳��
            app.UseAuthentication();//��֤������˭
                                    //��������֤,�������е�HTTP���� ���û����token��֤
                                    // app.AddAuthorize();
            app.UseAuthorization();//��Ȩ��������ʲô

            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("v1/swagger.json", "�����ĵ�һ����");
            //    c.SwaggerEndpoint("v2/swagger.json", "���ǽ�ӿ��ĵ�");
            //    //c.SwaggerEndpoint("/v1/api-docs", "LinCms");
            //});

            app.UseKnife4UI(c =>
            {
                c.RoutePrefix = ""; // serve the UI at root
                c.SwaggerEndpoint("/v1/swagger.json", "�ӿ��ĵ�");
                //c.SwaggerEndpoint("v2/swagger.json", "���ǽ�ӿ��ĵ�");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSwagger("{documentName}/swagger.json");
            });


            //���ÿ���
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        }
    }
}
