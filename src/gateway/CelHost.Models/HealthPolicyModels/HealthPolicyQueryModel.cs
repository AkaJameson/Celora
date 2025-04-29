namespace CelHost.Models.HealthPolicyModels
{
    public class HealthPolicyQueryModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int PageSize { get; set; } = 1;
        public int PageIndex { get; set; } = 20;
    }
}
