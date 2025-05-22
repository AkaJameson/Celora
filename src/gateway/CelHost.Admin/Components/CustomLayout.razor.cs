using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace CelHost.Admin.Components
{
    public partial class CustomLayout
    {
        [Parameter]
        public bool ShowFooter { get; set; } = false;

        [Parameter]
        [NotNull]
        public RenderFragment Header { get; set; } = null!;

        [Parameter]
        [NotNull]
        public RenderFragment Sidebar { get; set; } = null!;

        [Parameter]
        [NotNull]
        public RenderFragment Main { get; set; } = null!;

        [Parameter]
        public RenderFragment? Footer { get; set; }

        protected override Task OnParametersSetAsync()
        {
            if (ShowFooter && Footer == null)
            {
                throw new ArgumentNullException(nameof(Footer));
            }
            return base.OnParametersSetAsync();
        }
    }
}
