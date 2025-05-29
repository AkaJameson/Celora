namespace CelHost.Server.Dto
{
    public class SystemDictDto
    {
        public List<DictItem> Items { get; set; } = new();

    }
    public class DictItem
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string TypeCode { get; set; }
        public string Remark { get; set; }
        public List<DictItem> Child { get; set; } = new();
    }
}
