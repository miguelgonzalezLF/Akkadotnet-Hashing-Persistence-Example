using System.Configuration;
using System.Net;
using IEP.Host.Services;
using ServiceStack.Api.Swagger;
using ServiceStack.Razor;
using ServiceStack.ServiceInterface.Cors;
using ServiceStack.WebHost.Endpoints;
using Container = Funq.Container;

namespace IEP.Host
{
    public class RESTServiceHost : SmartThreadPoolHttpListener
    {
        public RESTServiceHost() : base("Test Server", typeof(BaseService).Assembly) { }

        public override void Configure(Container container)
        {
            container.Adapter = new StructureMapContainerAdapter();

            SetConfig(new EndpointHostConfig
            {
                WebHostUrl = ConfigurationManager.AppSettings.Get("REST_ServiceURL"),
                GlobalResponseHeaders = {
                                           {"Access-Control-Allow-Origin", "*"},
                                           {"Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"},
                                           {"Access-Control-Allow-Headers", "X-File-Name, X-File-Type, X-File-Size"}
                                   },
                CustomHttpHandlers = {
                                        { HttpStatusCode.NotFound, new RazorHandler("/notfound") },
                                        { HttpStatusCode.Forbidden, new RazorHandler("/forbidden") }
                                   },
            });

            Plugins.Add(new CorsFeature());
            Plugins.Add(new RazorFormat());
            Plugins.Add(new SwaggerFeature());
        }
    }
}