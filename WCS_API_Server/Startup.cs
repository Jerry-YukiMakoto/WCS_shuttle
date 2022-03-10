using System;
using System.Net.Http.Headers;
using System.Web.Http;
using Owin;
using Unity;

namespace WCS_API_Server
{
    public class Startup
    {
        private readonly UnityContainer _container;

        public Startup(UnityContainer container)
        {
            _container = container;
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.DependencyResolver = new UnityResolver(_container);
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}", 
                defaults: new { id = RouteParameter.Optional }
            );
            appBuilder.UseWebApi(config);
        }
    }
}

#region Route Template
//去抓ApiEventController中的__Controller的開頭
//E.g. [Route("WCS/MOVE_TASK")]的{controller}是WCSController
//id是function name
#endregion