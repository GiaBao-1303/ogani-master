using Microsoft.AspNetCore.Mvc.Rendering;

namespace ogani_master.Helpers
{
    public static class HtmlHelpers
    {
        public static string IsActive(this IHtmlHelper html, string controller, string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = routeData.Values["action"]?.ToString();
            var routeController = routeData.Values["controller"]?.ToString();

            return (controller == routeController && action == routeAction) ? "active" : string.Empty;
        }

        public static string IsParentActive(this IHtmlHelper html, params string[] controllers)
        {
            var routeData = html.ViewContext.RouteData;
            var currentController = routeData.Values["controller"]?.ToString();

            if (controllers.Contains(currentController))
            {
                return "active";
            }
            return string.Empty;
        }
    }

}
