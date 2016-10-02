using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CSVCompressor.Startup))]
namespace CSVCompressor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
