using Microsoft.AspNetCore.Components;

namespace CelHost.Admin.Components
{
    public partial class CustomNevMenu
    {
        [Parameter]
        public List<NavItem> NavItems { get; set; } = new();
        [Parameter]
        public RenderFragment Title { get; set; }
        [Inject]
        public NavigationManager NavManager { get; set; }
        private bool IsCollapsed = false;

        private void Toggle(NavItem item)
        {
            item.IsExpanded = !item.IsExpanded;
        }
        private void Toggle()
        {
            NavItems.ForEach(x => x.IsExpanded = false);
            IsCollapsed = !IsCollapsed;
        }

        public class NavItem
        {
            public string Title { get; set; } = "";
            public string Href { get; set; } = "";
            public string Icon { get; set; } = "bi bi-dot";
            public bool IsExpanded { get; set; } = false;
            public List<NavItem>? Children { get; set; }
        }
        private void NavigateTo(string href)
        {
            NavManager.NavigateTo(href);
        }

        private bool IsActive(string href)
        {
            var current = NavManager.ToBaseRelativePath(NavManager.Uri);
            return current.Equals(href.TrimStart('/'), StringComparison.OrdinalIgnoreCase);
        }
    }
}
