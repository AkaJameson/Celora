namespace CelHost.Models.HealthPolicyModels
{
    public class HealthPolicyAddModel
    {
        public string Name { get; set; }
        public int Interval { get; set; } = 120;
        public int TimeOut { get; set; } = 30;
        public string Path { get; set; }
    }
}
