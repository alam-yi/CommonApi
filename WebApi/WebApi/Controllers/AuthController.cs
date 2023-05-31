using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using PublicTool;
using System;
using System.Net.Http;
using static Model.JwtModel;

namespace WebApi
{
    //权限验证控制器   验证时候在header头部添加参数 Authorization   输入bearer + token
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[AuthAttribute]
    //[ApiExplorerSettingsAttribute(IgnoreApi = true)]//lgnoreApi表示是否显示
    public class AuthController : BaseApiController
    {
        [AllowAnonymous]//指定此属性应用于的类或方法不需要授权。
        [HttpGet]
        //[ApiExplorerSettingsAttribute(IgnoreApi = true)]

        public HttpResponseMessage LoginToToken(string userName, string pwd)
        {
            //登录后将token存储到redis
            //每次登录进行过期验证，少于半小时则刷新token
            OperateResultModel srm = new OperateResultModel();
            HttpResponseMessage responseMsg = new HttpResponseMessage();
            JwtTool _jwtTool = new JwtTool();
            ResultJwtModel _resultJwtModel = new ResultJwtModel();
            try
            {
                //逻辑操作
                //此处只简单的验证用户名和密码的不为空
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(pwd))
                {
                    //做用户信息判断
                    srm.ReturnCode = 1;
                    srm.ReturnMsg = "成功";
                    srm.ReturnData = _jwtTool.GetToken(userName, pwd);
                }
                else
                {
                    srm.ReturnCode = 1;
                    srm.ReturnMsg = "用户或密码错误";
                }
                responseMsg = base.CreateHttpResponse(JsonTool.SerializeObject(srm));
                return responseMsg;
            }
            catch (Exception ex)
            {
                srm.ReturnCode = -9999;
                srm.ReturnMsg = ex.Message;
                responseMsg = base.CreateHttpResponse(JsonTool.SerializeObject(srm));
                return responseMsg;
            }
        }
    }
}
