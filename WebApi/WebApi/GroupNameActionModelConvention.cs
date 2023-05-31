using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApi
{
    //IActionModelConvention方式一  实现全局接口分组
    public class GroupNameActionModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action.Controller.ControllerName == "TestGroup")
            {
                if (action.ActionName == "group_one")
                {
                    action.ApiExplorer.GroupName = "v1";
                    action.ApiExplorer.IsVisible = true;
                }
                else if (action.ActionName == "group_threee")
                {
                    action.ApiExplorer.GroupName = "v2";
                    action.ApiExplorer.IsVisible = true;
                }
            }
        }
    }
    //IControllerModelConvention方式二  实现全局接口分组
    public class GroupNameActionControllerConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerName == "TestGroup")
            {
                foreach (var action in controller.Actions)
                {
                    switch (action.ActionName)
                    {
                        case "group_one":
                            action.ApiExplorer.GroupName = "v1";
                            action.ApiExplorer.IsVisible = true;
                            break;
                        case "group_two":
                            action.ApiExplorer.GroupName = "v1";
                            action.ApiExplorer.IsVisible = true;
                            break;
                        case "group_threee":
                            action.ApiExplorer.GroupName = "v1";
                            action.ApiExplorer.IsVisible = true;
                            break;
                            //default: ""; break;
                    }
                }
            }
            //新增的控制器
            else if (controller.ControllerName == "Test")
            {
                foreach (var action in controller.Actions)
                {
                    switch (action.ActionName)
                    {
                        //新增的方法名
                        case "GetConst":
                            action.ApiExplorer.GroupName = "v1";
                            action.ApiExplorer.IsVisible = true;
                            break;

                            //default: ""; break;
                    }

                }
            }
            else if (controller.ControllerName == "Home")
            {
                foreach (var action in controller.Actions)
                {
                    switch (action.ActionName)
                    {
                        case "Get":
                            action.ApiExplorer.GroupName = "v1";
                            action.ApiExplorer.IsVisible = true;
                            break;
                        case "Get2":
                            action.ApiExplorer.GroupName = "v1";
                            action.ApiExplorer.IsVisible = true;
                            break;

                            //default: ""; break;
                    }
                }
            }
            else if (controller.ControllerName == "Auth")
            {
                foreach (var action in controller.Actions)
                {
                    switch (action.ActionName)
                    {
                        case "LoginToToken":
                            action.ApiExplorer.GroupName = "v2";
                            action.ApiExplorer.IsVisible = true;
                            break;


                            //default: ""; break;
                    }
                }
            }
        }
    }
}
