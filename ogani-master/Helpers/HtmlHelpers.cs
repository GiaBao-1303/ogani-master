using Microsoft.AspNetCore.Mvc.Rendering;

namespace ogani_master.Helpers
{

    public struct ControllerInfo
    {
        public string action;
        public string controllerName;
    }

    public static class HtmlHelpers
    {
        public static string IsActive(this IHtmlHelper html, string controller, string action)
        {
            var routeData = html.ViewContext.RouteData;

            string routeAction = routeData.Values["action"]?.ToString();
            string routeController = routeData.Values["controller"]?.ToString();

            return (controller == routeController && action == routeAction) ? "active" : string.Empty;
        }

        public static string IsParentActive(this IHtmlHelper html, params ControllerInfo[] controllerInfos)
        {
            var routeData = html.ViewContext.RouteData;
            string currentController = routeData.Values["controller"]?.ToString();
            string currentAction = routeData.Values["action"]?.ToString();

            foreach (ControllerInfo info in controllerInfos)
            {
                if (string.Equals(info.controllerName, currentController, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(info.action, currentAction, StringComparison.OrdinalIgnoreCase))
                {
                    return "active";
                }
            }

            return string.Empty;
        }
    }

}
