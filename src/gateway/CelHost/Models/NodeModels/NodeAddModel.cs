namespace CelHost.Models.NodeModels
{
    /// <summary>
    /// 添加节点
    /// </summary>
    public class NodeAddModel
    {
        public int ClusterId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
    }

}
