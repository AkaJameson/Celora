using CelHost.Database;

namespace CelHost.ServicesImpl
{
    public class HealthCheckServiceImpl
    {
        private readonly HostContext hostContext;
        public HealthCheckServiceImpl(HostContext hostContext)
        {
            this.hostContext = hostContext;
        }


    }
}
