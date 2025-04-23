using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.SystemDictModels
{
    public class QueryDictModel
    {
        [Required]
        public string typeName { get; set; }
        [Required]
        public int PageIndex { get; set; } = 1;
        [Required]
        public int PageSize { get; set; } = 20;

    }
}
