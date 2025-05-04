namespace CelHost.Apis.Models
{
    public class FileInfoDto
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public string LastModified { get; set; }
    }
}
