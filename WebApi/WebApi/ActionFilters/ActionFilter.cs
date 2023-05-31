using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Model;
using System.Collections.Generic;
using System.Text;

namespace WebApi
{
    /// <summary>
    /// 方法过滤器
    /// </summary>
    public class ActionFilter : IActionFilter
    {
        /// <summary>
        /// 监控日志
        /// </summary>
        public static ILogger LoggerMonitor { get; set; }
        /// <summary>
        /// 错误日志
        /// </summary>
        public static ILogger LoggerError { get; set; }

        /// <summary>
        /// 创建请求日志文本
        /// </summary>
        /// <param name="method">请求方法</param>
        /// <param name="controllerName">控制器名称</param>
        /// <param name="actionName">方法名称</param>
        /// <param name="actionArgs">方法参数</param>
        /// <returns></returns>
        private string CreateRequestLogText(string method, string controllerName, string actionName, IDictionary<string, object> actionArgs)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"收到请求[{method}]/{controllerName}/{actionName}，参数：");
            if (actionArgs.Count > 0)
            {
                foreach (var p in actionArgs)
                {
                    sb.AppendLine($"    " + p.Key + "：" + Newtonsoft.Json.JsonConvert.SerializeObject(p.Value));
                }
            }
            else
            {
                sb.AppendLine("    无");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 创建响应日志文本
        /// </summary>
        /// <param name="method">请求方法</param>
        /// <param name="controllerName">控制器名称</param>
        /// <param name="actionName">方法名称</param>
        /// <param name="result">执行结果</param>
        /// <returns></returns>
        private string CreateResponseLogText(string method, string controllerName, string actionName, object result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"完成请求[{method}]/{controllerName}/{actionName}，结果：");
            if (result != null)
            {
                sb.AppendLine("    " + Newtonsoft.Json.JsonConvert.SerializeObject(result));
            }
            else
            {
                sb.AppendLine("    无");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 方法执行前
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (LoggerMonitor != null)
            {
                // 记录请求参数日志
                ControllerActionDescriptor desc = context.ActionDescriptor as ControllerActionDescriptor;
                if (desc != null)
                {
                    var logText = CreateRequestLogText(
                        context.HttpContext.Request.Method,
                        desc.ControllerName,
                        desc.ActionName,
                        context.ActionArguments);
                    LoggerMonitor.LogDebug(logText);
                }
            }
        }

        /// <summary>
        /// 方法执行后
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            ObjectResult rst = context.Result as ObjectResult;
            object rstValue = rst != null ? rst.Value : null;
            if (context.Exception != null)
            {
                // 异常处理
                context.ExceptionHandled = true;
                if (context.Exception is UserFriendlyException)
                {
                    // 如果是用户异常
                    context.HttpContext.Response.StatusCode = 200;
                    context.Result = new ObjectResult(new OperateResultModel(context.Exception.Message, rstValue, 1));
                }
                else
                {
                    // 如果是系统异常，禁止返回异常的详细信息
                    context.HttpContext.Response.StatusCode = 500;
                    context.Result = new ContentResult() { Content = context.Exception.Message };
                    if (LoggerError != null)
                    {
                        LoggerError.LogError(context.Exception, context.Exception.Message);
                    }
                }
            }
            else
            {
                // 无异常
                context.Result = new ObjectResult(new OperateResultModel(string.Empty, rstValue, 1));
            }

            if (LoggerMonitor != null)
            {
                // 记录请求结果日志
                ControllerActionDescriptor desc = context.ActionDescriptor as ControllerActionDescriptor;
                if (desc != null)
                {
                    var logText = CreateResponseLogText(
                        context.HttpContext.Request.Method,
                        desc.ControllerName,
                        desc.ActionName,
                        rstValue);
                    LoggerMonitor.LogDebug(logText);
                }
            }
        }
    }
}