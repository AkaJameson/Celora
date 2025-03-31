using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CelHost.Comm.Client
{
    public class ProxyConfiguration
    {
        /// <summary>
        /// 网关地址
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Uri HostAddress { get; set; }
        /// <summary>
        /// 集群id
        /// </summary>
        public int ClusterId { get; set; }
        /// <summary>
        /// 节点地址
        /// </summary>
        [Required(ErrorMessage = "NodeUrl 不能为空")]
        public string NodeUrl { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        [Required(ErrorMessage = "Port 不能为空")]
        public string Port { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsActive { get; set; } = true;

        public void Validate()
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);
            bool isValid = Validator.TryValidateObject(this, validationContext, validationResults, true);
            if (!isValid)
            {
                throw new ValidationException(validationResults.First().ErrorMessage);
            }
        }
    }
}
