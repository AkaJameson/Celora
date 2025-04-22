using System.ComponentModel.DataAnnotations;

namespace CelHost.Models
{
    public class BlockModel
    {
        [Required]
        public string Ip { get; set; }

        [Required]
        public string Reason { get; set; }
    }
    public class UnblockModel
    {
        [Required]
        public string Ip { get; set; }
    }
}
