namespace CelHost.Models.Gateway
{
    public class CascadeQueryModel
    {
        public string? Name { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
