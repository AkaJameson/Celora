using BootstrapBlazor.Components;

namespace CelHost.Admin.Models
{
    public class Menus
    {
        public static List<MenuItem> menuItems = new List<MenuItem>()
        {
            new MenuItem()
            {
                Icon = "fa fa-cogs",
                Text = "系统设置",
                Items = new List<MenuItem>()
                {
                    new MenuItem()
                    {
                        Icon = "fa fa-cog",
                        Text = "系统配置",
                        Url = "/system/config"
                    },
                    new MenuItem()
                    {
                        Icon = "fa fa-ban",
                        Text = "黑名单管理",
                        Url = "/system/blacklist"
                    },
                    new MenuItem()
                    {
                        Icon = "fa fa-book",
                        Text = "数据字典",
                        Url = "/system/dictionary"
                    }
                }
            },
            new MenuItem()
            {
                Icon = "fa fa-server",
                Text = "服务管理",
                Items = new List<MenuItem>()
                {
                    new MenuItem()
                    {
                        Icon = "fa fa-route",
                        Text = "路由管理",
                        Url = "/services/routes"
                    },
                    new MenuItem()
                    {
                        Icon = "fa fa-network-wired",
                        Text = "节点管理",
                        Url = "/services/nodes"
                    }
                }
            },
            new MenuItem()
            {
                Icon = "fa fa-network-wired",
                Text = "网关管理",
                Items = new List<MenuItem>()
                {
                    new MenuItem()
                    {
                        Icon = "fa fa-eye",
                        Text = "网关查看",
                        Url = "/gateway/view"
                    },
                    new MenuItem()
                    {
                        Icon = "fa fa-plus",
                        Text = "网关添加",
                        Url = "/gateway/add"
                    }
                }
            },
            new MenuItem()
            {
                Icon = "fa fa-file-alt",
                Text = "日志管理",
                Items = new List<MenuItem>()
                {
                    new MenuItem()
                    {
                        Icon = "fa fa-file-search",
                        Text = "日志查看",
                        Url = "/logs/view"
                    },
                    new MenuItem()
                    {
                        Icon = "fa fa-trash",
                        Text = "日志清理",
                        Url = "/logs/clean"
                    }
                }
            }
        };
    }
}
