using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.Gateway
{
    public class GatewayDetalModel
    {
        public int GatewayId { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
