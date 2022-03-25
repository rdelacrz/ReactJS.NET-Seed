using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using JavaScriptEngineSwitcher.Core;
using Application.Filters;

namespace Application
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Sets up Web API portions of application
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Sets up MVC portions of application
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            JsEngineSwitcherConfig.Configure(JsEngineSwitcher.Current);

            // Registers exception filter for validations
            GlobalConfiguration.Configuration.Filters.Add(new ApiExceptionFilterAttribute());
        }
    }
}
