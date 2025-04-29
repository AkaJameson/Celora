using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.Gateway
{
    public class CascadeAddModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Source { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
