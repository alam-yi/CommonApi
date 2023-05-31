using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Model.JwtModel;

namespace PublicTool
{
    /// <summary>
    /// jwt工具类
    /// </summary>
    public class JwtTool
    {
        //在JWT机制下，登录用户的信息保存在客户端，服务器端不需要保存数据，这样我们的程序就天然地适合分布式的集群环境，而且服务器端从客户端请求中就可以获取当前登录用户的信息，不需要再去状态服务器中获取，因此程序的运行效率更高。虽然用户信息保存在客户端，但是由于有签名的存在，客户端无法篡改这些用户信息，因此可以保证客户端提交的JWT的可信度。
        private string _SecurityKey;//秘钥 注意：密钥长度需要大于16位
        private string _Issuer;//jwt签发者
        private string _Audience;//接收jwt的一方 
        private int _Expires;//jwt的过期时间，这个过期时间必须要大于签发时间

        public JwtTool()
        {
            _SecurityKey = "5eJYQqyEmoDfDCU88w0bDvFTDnKw0rr5+AgwnarW5dqYk4nlJPWRhLeab4gqd39fC+c9TP8jBJQU3HVcuUqSHQhlOuGn2UoIyw+hjJSy21DpXXlEIiSXs5gMscDo5edWwZa+jZ6sPhGSuMSnEkVAOgUSIsYSyOfgfUNZ";
            _Expires = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Expires"]);//常数，随意更改 单位秒
            _Issuer = "http://localhost:5000";
            _Audience = "http://localhost:5000";
        }
        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="loginPassword"></param>
        /// <param name="loginIp"></param>
        /// <param name="loginId"></param>
        /// <returns></returns>
        public ResultJwtModel GetToken(string loginName, string loginId)
        {
            RedisTool _redisTool = new RedisTool();
            ResultJwtModel _jwtModel = new ResultJwtModel();
            #region 有效载荷， 可以自己写，爱写多少写多少;尽量避免敏感信息
            //获取IP信息
            IPTool _ipTool = new IPTool();
            //返回给前端的token
            var cls_Access = new[]
            {
                new Claim("loginName",loginName),
                //new Claim("loginPassword",loginPassword), 敏感信息不要放在token里
                new Claim("loginIp",_ipTool.GetIp()),
                new Claim("loginId",loginId),
                 new Claim("loginTokenType","Access")
            };
            //刷新令牌token 过期时间为访问token一倍
            var cls_Refresh = new[]
           {
                new Claim("loginName",loginName),
                //new Claim("loginPassword",loginPassword),敏感信息不要放在token里
                new Claim("loginIp",_ipTool.GetIp()),
                new Claim("loginId",loginId),
                 new Claim("loginTokenType","Refresh")
            };
            #endregion

            //加密  
            //依赖Nuget：Microsoft.IdentityModel.Tokens
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_SecurityKey));
            //签名
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //依赖Nuget：System.IdentityModel.Tokens.Jwt 
            var jwtSecurity_AccessToken = new JwtSecurityToken(
                   issuer: _Issuer,//jwt签发者
                   audience: _Audience,//接收jwt的一方 
                   notBefore: DateTime.Now,//定义在什么时间之前，该jwt都是不可用的
                   expires: DateTime.Now.AddSeconds(_Expires),//jwt的过期时间，这个过期时间必须要大于签发时间
                   claims: cls_Access,
                   signingCredentials: creds
                );
            var jwtSecurity_RefreshToken = new JwtSecurityToken(
                  issuer: _Issuer,//jwt签发者
                  audience: _Audience,//接收jwt的一方 
                  notBefore: DateTime.Now,//定义在什么时间之前，该jwt都是不可用的
                  expires: DateTime.Now.AddSeconds(_Expires * 2),//刷新令牌时间为访问令牌时间一倍
                  claims: cls_Refresh,
                  signingCredentials: creds
               );
            _jwtModel.ExpireTime = DateTime.Now.AddSeconds(_Expires);
            _jwtModel.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurity_AccessToken);
            _jwtModel.RefreshToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurity_RefreshToken);
            //将刷新令牌写入redis
            //Task t = new Task(() =>
            //{            
            _redisTool.InsertStrExpire(loginId, _jwtModel.RefreshToken, ((_Expires * 2) / 60));
            //});

            return _jwtModel;
        }

        /// <summary>
        /// 对jwt字符串 解码
        /// </summary> 
        public Dictionary<string, string> DecodeJwt(string jwtStr)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            var jwtHandler = new JwtSecurityTokenHandler();
            // token校验
            if (!string.IsNullOrEmpty(jwtStr) && jwtHandler.CanReadToken(jwtStr))
            {
                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);

                var claims = jwtToken.Claims;

                foreach (var claim in claims)
                {
                    dic.Add(claim.Type, claim.Value);
                }
            }
            return dic;
        }


        /// <summary>
        /// 校验解析token
        /// </summary>
        /// <returns></returns>
        public static string ValidateJwtToken(string token, string secret)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm alg = new HMACSHA256Algorithm();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, alg);
                var json = decoder.Decode(token, secret, true);
                //校验通过，返回解密后的字符串
                return json;
            }
            catch (TokenExpiredException)
            {
                //表示过期
                return "expired";
            }
            catch (SignatureVerificationException)
            {
                //表示验证不通过
                return "invalid";
            }
            catch (Exception)
            {
                return "error";
            }
        }


        #region 手写地处加密逻辑
        //        IPTool _ipTool = new IPTool();
        //        DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
        //            //此处只简单的验证用户名和密码的不为空，实际中使用时不要这样
        //            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(pwd))
        //            {
        //                //Header
        //                var header = "{\"alg\": \"HS256\",\"typ\": \"JWT\"}";
        //        var headerBase = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(header));//Base64转码

        //        //Payload
        //        var payloadDic = new Dictionary<string, string>();
        //        payloadDic["iss"] = Const.Domain;//代表这个JWT的签发主体；
        //                payloadDic["aud"] = Const.Domain;//代表这个JWT的接收对象； 
        //                payloadDic["sub"] = "bbq_user_id";//当前登陆人的ID
        //                //添加jwt可用时间

        //                payloadDic["nbf"] = Convert.ToString(Convert.ToInt32((DateTime.Now - dateStart).TotalSeconds));//可用时间起始
        //                payloadDic["exp"] = Convert.ToString(Convert.ToInt32((DateTime.Now.AddMinutes(3) - dateStart).TotalSeconds));//可用时间结束
        //                payloadDic["ip"] = _ipTool.GetIp();//IP 防止异地登录泄露信息,增加安全性 可以提示告知用户是否开启安全模式，安全模式下则不绑定IP，可以任意地址登录,不用刷新token
        //                payloadDic["jti"] = _ipTool.GetIp();//JWT的唯一标识。
        //                var payload = JsonConvert.SerializeObject(payloadDic);
        //        var payloadBase = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(payload));//Base64转码


        //        //Signature
        //        //声明hs256对象
        //        var hs256 = new HMACSHA256(Encoding.UTF8.GetBytes(Const.SecurityKey));
        //        //生成signature
        //        var signature = hs256.ComputeHash(Encoding.UTF8.GetBytes(headerBase + "." + payloadBase));
        //        var signatureBase = Base64UrlTextEncoder.Encode(signature);
        //                return Ok(new
        //                {
        //                    token = headerBase + "." + payloadBase + "." + signatureBase
        //    });
        //            }
        //            else
        //{
        //    return BadRequest(new { message = "username or password is incorrect." });
        //}
        #endregion
    }
}
