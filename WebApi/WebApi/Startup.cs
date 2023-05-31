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
                //过滤器，访问接口必须走此特性 全局接口都需要权限验证 除非添加AllowAnonymous
                option.Filters.Add<AuthExtension>();
                //option.Filters.Add<AuthAttribute>();
            }).AddNewtonsoftJson(options =>
            {
                //全局日期格式化
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //返回的Json大小写原样输出
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //取消返回Josn数据默认首字母小写
                options.UseMemberCasing();
            });

            //方式一全局实现接口归属ApiExplorerSettingsAttribute ，这样接口上不用再写
            //services.AddControllers(options =>
            //{
            //    options.Conventions.Add(new GroupNameActionModelConvention());
            //});
            //方式二全局实现接口归属ApiExplorerSettingsAttribute ，这样接口上不用再写
            services.AddControllers(options =>
            {
                options.Conventions.Add(new GroupNameActionControllerConvention());
            });


            //注入JWT验证依赖服务  AddAuthentication注册身份认证服务 参数为认证类型
            //AddJwtBearer  配置jwt认证  参数为委托
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                //获取密钥
                var secretByte = Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //验证token发布者  只有后端Issuer发出的Token才被接受
                    ValidateIssuer = true,
                    ValidIssuer = Const.Domain,// Configuration["Authentication:Issuer"],
                    //token持有者
                    ValidateAudience = true,
                    ValidAudience = Const.Domain,// Configuration["Authentication:Audience"],
                    //验证token是否过期
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    //传如私钥并加密
                    IssuerSigningKey = new SymmetricSecurityKey(secretByte)

                };
            });
            // services.AddScoped<IAuthorizationHandler, AuthExtension>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            ////启用接口统一返回过滤器
            services.AddMvc(options =>
            {
                // options.Filters.Add(typeof(ActionFilterTool));

            }).AddWebApiConventions();//添加此属性HttpResponseMessage 才能正常返回数据

            //注册swagger服务
            #region Knife4Jui
            services.Configure<Knife4UIOptions>(options =>
            {
                Configuration.Bind("Knife4UI", options);
            });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "接口文档",
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Name = "溪儿",
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

                //接口地址可多选
                options.AddServer(new OpenApiServer() { Url = "http://localhost:8085", Description = "地址1" });
                options.AddServer(new OpenApiServer() { Url = "http://127.0.0.1:5001", Description = "地址2" });
                //192.168.28.213是我本地IP
                options.AddServer(new OpenApiServer() { Url = "http://192.168.28.213:5002", Description = "地址3" });

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebApi.xml");
                options.IncludeXmlComments(filePath, true);

                //给swagger全局接口添加JwtBearer认证
                //定义JwtBearer认证方式一 Authorization
                options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme()
                {
                    Description = "这是方式一(直接在输入框中输入认证信息，不需要在开头添加Bearer)",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                //    //定义JwtBearer认证方式二
                //    //options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme()
                //    //{
                //    //    Description = "这是方式二(JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）)",
                //    //    Name = "Authorization",//jwt默认的参数名称
                //    //    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                //    //    Type = SecuritySchemeType.ApiKey
                //    //});

                //声明一个Scheme，注意下面的Id要和上面AddSecurityDefinition中的参数name一致
                var scheme = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "JwtBearer" }
                };
                //注册全局认证（所有的接口都可以使用认证）
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

            //预处理判断 ，类似debug
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //请求重定向的中间件
            //app.UseHttpsRedirection();
            app.UseRouting();//路由，你在哪

            //添加jwt验证 此管道执行有先后顺序
            app.UseAuthentication();//认证，你是谁
                                    //添加身份验证,拦截所有的HTTP请求 做用户身份token验证
                                    // app.AddAuthorize();
            app.UseAuthorization();//授权，你能做什么

            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("v1/swagger.json", "测试文档一无用");
            //    c.SwaggerEndpoint("v2/swagger.json", "表白墙接口文档");
            //    //c.SwaggerEndpoint("/v1/api-docs", "LinCms");
            //});

            app.UseKnife4UI(c =>
            {
                c.RoutePrefix = ""; // serve the UI at root
                c.SwaggerEndpoint("/v1/swagger.json", "接口文档");
                //c.SwaggerEndpoint("v2/swagger.json", "表白墙接口文档");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapSwagger("{documentName}/swagger.json");
            });


            //设置跨域
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        }
    }
}
