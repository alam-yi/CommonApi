using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace WebApi
{
    //继承这两属性就可以当做过滤器
    public class AuthAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {

        }
    }
}