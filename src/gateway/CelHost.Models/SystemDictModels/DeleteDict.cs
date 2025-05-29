using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.SystemDictModels
{
    public class DeleteDict
    {
        [Required]
        public List<int> Id { get; set; }
    }
}
