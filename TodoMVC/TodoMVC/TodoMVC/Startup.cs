using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TodoMVC.Startup))]
namespace TodoMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
