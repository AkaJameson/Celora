namespace CelHost.Models.ClusterModels
{
    public class ClusterQueryModel
    {
        public string Name { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
