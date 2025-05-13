using System;

namespace CelHost.Admin.Shared.Models
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsExpanded { get; set; } = false;
        public List<SubMenuItem> SubItems { get; set; }
    }
    public class SubMenuItem
    {
        /// <summary>
        /// 子菜单项名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        private string url;
        /// <summary>
        /// 子菜单项导航链接
        /// </summary>
        public string Url { get { return url; } set { url = value.StartsWith("/") ? value : ($"/{value}"); } }
    }
}
