using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiService
{
    using App_Start;
    using Owin;
    using System.Web.Http;

    public class Startup : IOwinAppBuilder
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            FormatterConfig.ConfigureFormatters(config.Formatters);

            appBuilder.UseWebApi(config);
        }
    }
}
