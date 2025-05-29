using BootstrapBlazor.Components;
using CelHost.Admin.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CelHost.Admin.Layout
{
    public partial class MainLayout
    {
        [Inject]
        private ILocalStorageService localStorage { get; set; }
        [Inject]
        private FullScreenService fullScreenService { get; set; }
        private string UserName { get; set; }
        private string Account { get; set; }
        private bool isfullScreen { get; set; } = false;

        private void Toggle()
        {
            fullScreenService.Toggle();
            isfullScreen = !isfullScreen;
        }
        protected override async Task OnInitializedAsync()
        {
            UserName = await localStorage.GetItemAsync("userName");
            Account = await localStorage.GetItemAsync("account");
        }
        private List<CustomNevMenu.NavItem> NavLinks = new()
    {
        new()
        {
            Title = "仪表盘",
            Icon = "bi bi-speedometer2",
            Href = "/dashboard"
        },
        new()
        {
            Title = "系统管理",
            Icon = "bi bi-gear",
            IsExpanded = true,
            Children = new()
            {
                new CustomNevMenu.NavItem { Title = "用户管理", Href = "/System/Dictionary", Icon = "bi bi-person" },
                new CustomNevMenu.NavItem { Title = "角色管理", Href = "/roles", Icon = "bi bi-shield-lock" }
            }
        },
        new()
        {
            Title = "关于",
            Icon = "bi bi-info-circle",
            Href = "/about"
        }
    };
    }
}
