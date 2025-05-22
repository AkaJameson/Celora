using Microsoft.AspNetCore.Components;

namespace CelHost.Admin.Components
{
    public partial class PageBase
    {
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }
}
