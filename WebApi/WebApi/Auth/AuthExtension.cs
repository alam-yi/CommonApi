using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Model;
using PublicTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WebApi
{
    public class AuthExtension : IActionFilter   //Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// 通过IHttpContextAccessor可以获取HttpContext相关信息，但一定要注册服务
        /// </summary>
        // private readonly IHttpContextAccessor _accessor;
        /// <summary>
        /// 用于判断请求是否带有凭据和是否登录
        /// </summary>
        //   public IAuthenticationSchemeProvider Scheme { get; set; }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 验证Token的正确性和是否过期
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            //&& context.HttpContext.Session.GetString("User") == null
            if (HasAllow(context) == false)
            {
                if (context.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    var authorize = context.HttpContext.Request.Headers["Authorization"].ToString();
                    if (authorize.Contains("Bearer"))
                    {
                        var info = authorize.Replace("Bearer ", string.Empty);
                        var jwtStr = info.Split('.').ToArray();
                        //声明hs256对象
                        var hs256 = new HMACSHA256(Encoding.UTF8.GetBytes(Const.SecurityKey));
                        //生成signature
                        var signature = Base64UrlTextEncoder.Encode(hs256.ComputeHash(Encoding.UTF8.GetBytes(jwtStr[0] + "." + jwtStr[1])));
                        //验证加密后是否相等
                        if (jwtStr[2] == signature)
                        {
                            //得到解析后的数据
                            RedisTool _redisTool = new RedisTool();
                            var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(jwtStr[1])));

                            //验证是否在有效时间内
                            JwtTool _jwtTool = new JwtTool();
                            var now = DateTime.UtcNow.ToUniversalTime();
                            int userId = Convert.ToInt32(payload["loginId"]);//用户id
                            var redisToken = _redisTool.ReadStr(userId.ToString());
                            //这样可以避免异地登录时候作废的token在前端被人恶意拿去调用
                            if (redisToken != null)
                            {
                                if (now >= Convert.ToDateTime(payload["nbf"]) && now <= Convert.ToDateTime(payload["exp"]))
                                {
                                    //判断IP地址，是否异地登录
                                    IPTool _ipTool = new IPTool();
                                    if (payload["loginIp"].ToString() != _ipTool.GetIp())
                                    {
                                        context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("您的账号在异地登录，请确认账号信息并重新登录", "", 1)) };
                                        //注销token  此时前端应该删除token，避免被重复无意义调用
                                        _redisTool.DeleteStr(payload["loginId"].ToString());
                                        return;
                                    }

                                    //判断还有多久过期，是否需要刷新时间 提前5分钟刷新 主要用作用户正在操作时候避免时间到期影响用户体验
                                    if (now.AddMinutes(5) >= Convert.ToDateTime(payload["exp"]))
                                    {
                                        //用刷新令牌使访问令牌重新生成
                                        //可以选择延长刷新令牌，根据实际情况来定时间，是否延长看个人需求
                                        context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("Token错误!", _jwtTool.GetToken(payload["loginName"].ToString(), userId.ToString()), -20)) };//刷新token返回-20，让前端再调一次接口
                                        return;
                                    }
                                }
                                else
                                {
                                    //访问令牌过期 则判断刷新令牌是否过期 如果过期则重新登录  
                                    context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("Token错误!", _jwtTool.GetToken(payload["loginName"].ToString(), userId.ToString()), -20)) };//刷新token返回-20，让前端再调一次接口
                                    return;
                                }
                            }
                            else
                            {
                                context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("请重新登录!", "", -1)) };
                                return;
                            }

                        }
                        else
                        {
                            context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("Token错误!", "", -1)) };
                            return;
                        }
                    }
                    else
                    {
                        context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("Bearer缺失!", "", -1)) };
                        return;
                    }
                }
                else
                {
                    context.Result = new ContentResult { Content = JsonConvert.SerializeObject(new OperateResultModel("Token缺失!", "", -1)) };
                    return;
                }
            }
        }
        /// <summary>
        /// 排除掉控制器不需要鉴权  即加[AllowAnonymous]特性的无需鉴权
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool HasAllow(ActionExecutingContext context)
        {
            var filters = context.Filters;
            if (filters.OfType<IAllowAnonymousFilter>().Any())
            {
                return true;
            }
            var endpoint = context.HttpContext.GetEndpoint();
            return endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
        }





        //public  void AddAuthorize(this IApplicationBuilder applicationBuilder)
        //{
        //    applicationBuilder.Use(async (currentContext, nextContext) =>
        //    {
        //        //获取是否具有自定义的auth特性
        //        var authAttribute = currentContext.GetEndpoint()?.Metadata.GetMetadata<AuthAttribute>();
        //        if (authAttribute != null)
        //        {
        //            if (currentContext.Request.Headers.ContainsKey("Authorization"))
        //            {
        //                var authorize = currentContext.Request.Headers["Authorization"].ToString();
        //                if (authorize.Contains("Bearer"))
        //                {
        //                    var info = authorize.Replace("Bearer ", string.Empty);
        //                    var jwtStr = info.Split('.').ToArray();
        //                    //声明hs256对象
        //                    var hs256 = new HMACSHA256(Encoding.UTF8.GetBytes(Const.SecurityKey));
        //                    //生成signature
        //                    var signature = Base64UrlTextEncoder.Encode(hs256.ComputeHash(Encoding.UTF8.GetBytes(jwtStr[0] + "." + jwtStr[1])));
        //                    //验证加密后是否相等
        //                    if (jwtStr[2] == signature)
        //                    {
        //                        //验证是否在有效时间内
        //                        var now = DateTime.UtcNow.ToUniversalTime();
        //                        var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(jwtStr[1])));
        //                        if (now >= Convert.ToDateTime(payload["nbf"]) && now <= Convert.ToDateTime(payload["exp"]))
        //                        {
        //                            //await currentContext.Response.WriteAsync("验证通过").ConfigureAwait(true);
        //                            await nextContext?.Invoke();
        //                            return;
        //                        }
        //                        currentContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //                        await currentContext.Response.WriteAsync("Authorization time has passed, please log in again!").ConfigureAwait(true);
        //                    }
        //                }
        //            }
        //            currentContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //            //序列化返回错误信息
        //            string obj = JsonConvert.SerializeObject(new OperateResultModel(false, "验证失败!", ""));
        //            await currentContext.Response.WriteAsync(obj).ConfigureAwait(true);
        //        }
        //        await nextContext?.Invoke();
        //    });
        //}
    }

}
