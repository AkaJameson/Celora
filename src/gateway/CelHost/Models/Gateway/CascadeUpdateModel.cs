using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.Gateway
{
    public class CascadeUpdateModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string? Source { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }

    }
}
