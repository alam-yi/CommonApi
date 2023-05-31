using Microsoft.AspNetCore.Mvc;
using Model;
using PublicTool;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace WebApi    
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[Authorize] 注释掉的原因在于添加了这个，只有成功才会进到过滤器
    //Main主控制器
    //[AuthAttribute]
    //[AllowAnonymous]
    public partial class HomeController : BaseApiController
    {

        [HttpGet]
        //[ApiExplorerSettings(GroupName = "v2")]属于文档二
        public HttpResponseMessage Get2()
        {
            OperateResultModel srm = new OperateResultModel();
            HttpResponseMessage responseMsg = null;
            try
            {
                //逻辑操作

                srm.ReturnCode = 1;
                srm.ReturnMsg = "成功";
                srm.ReturnData = "fffff";
                responseMsg = base.CreateHttpResponse(JsonTool.SerializeObject(srm));
                return responseMsg;
            }
            catch (Exception ex)
            {
                srm.ReturnCode = -9999;
                srm.ReturnMsg = ex.Message;
                return responseMsg;
            }
        }

        [HttpGet]
        public HttpResponseMessage Test()
        {
            HttpResponseMessage response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("success", Encoding.GetEncoding("UTF-8"), "application/json")
            };
            return response;
        }
    }
}
