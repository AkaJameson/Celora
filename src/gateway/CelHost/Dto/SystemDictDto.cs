using CelHost.Data;

namespace CelHost.Dto
{
    public class SystemDictDTO
    {
        public int PkId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemValue { get; set; }
        public string ItemDesc { get; set; }
        public string Remark { get; set; }
        public int Order { get; set; }
        public string SuperTypeCode { get; set; }
    }

    // 在SystemDictDTO.cs中添加
    public class SystemDictTree: SystemDictDTO
    {
        public List<SystemDictTree> Children { get; set; } = new();
    }

}
