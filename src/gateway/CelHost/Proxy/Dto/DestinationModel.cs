using Yarp.ReverseProxy.Model;

namespace CelHost.Server.Proxy.Dto
{
    public class DestinationModel
    {
        public string DestinationId { get; set; }
        public DestinationHealth HealtState { get; set; }
    }
}
