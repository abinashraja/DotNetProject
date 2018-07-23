using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EPortal.Startup))]
namespace EPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           
        }
    }
}
