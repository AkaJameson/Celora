using System.ComponentModel.DataAnnotations;

namespace CelHost.Models.ClusterModels
{
    public class ClusterAddModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string PrefixPath { get; set; }
        [Required]
        public string RateLimitPolicyName { get; set; }
        [Required]
        public string LoadBalancePolicyName { get; set; }
        public bool IsActive { get; set; }
        public bool HealthCheck { get; set; }
        public int? HealthCheckId { get; set; }
    }
}
