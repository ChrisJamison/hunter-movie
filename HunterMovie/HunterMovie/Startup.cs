using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HunterMovie.Startup))]
namespace HunterMovie
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
