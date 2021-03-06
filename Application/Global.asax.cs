using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using JavaScriptEngineSwitcher.Core;
using Application.Filters;

namespace Application
{
    public class MainApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Sets up Web API portions of application
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Sets up MVC portions of application
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            JsEngineSwitcherConfig.Configure(JsEngineSwitcher.Current);

            // Registers global filters and exception filter for validations
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configuration.Filters.Add(new ApiExceptionFilterAttribute());

            // Sets up dependency injection via Autofac
            DependencyInjectionConfig.SetupDependencyInjection();
        }
    }
}
