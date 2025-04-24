namespace CelHost.Models.HealthPolicyModels
{
    public class HealthPolicyUpdateModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? Interval { get; set; }
        public int? TimeOut { get; set; }
        public string? Path { get; set; }
    }
}
