using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;

namespace WebApi
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[AuthExtension]
    [AllowAnonymous]
    public class BaseApiController : ControllerBase
    {
        [HttpGet]
        protected HttpResponseMessage CreateHttpResponse(string data)
        {
            HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(data, Encoding.GetEncoding("UTF-8"), "application/json") };
            return response;
        }
    }
}
