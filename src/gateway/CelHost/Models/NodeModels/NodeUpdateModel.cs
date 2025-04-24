namespace CelHost.Models.NodeModels
{
    public class NodeUpdateModel
    {
        public int ClusterId { get; set; }
        public int NodeId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public bool? isActive { get; set; }
    }
}
