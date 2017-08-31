using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AdvisementSoftware.Startup))]
namespace AdvisementSoftware
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
