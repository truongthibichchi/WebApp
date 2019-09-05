using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(webApp.Startup))]
namespace webApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
