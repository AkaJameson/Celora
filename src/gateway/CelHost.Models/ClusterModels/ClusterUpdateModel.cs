namespace CelHost.Models.ClusterModels
{
    public class ClusterUpdateModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? RateLimitPolicyName { get; set; }
        public string? LoadBlancePolicyName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsHealthCheck { get; set; }
        public int? HealthCheckId { get; set; }
    }
}
