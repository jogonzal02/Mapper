using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AllStateBenefits.Startup))]
namespace AllStateBenefits
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
