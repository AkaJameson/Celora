using BootstrapBlazor.Components;
using CelHost.Admin.Components;
using CelHost.Apis.ApiServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CelHost.Admin.Pages
{
    public partial class LoginPage
    {
        [Inject]
        [NotNull]
        private DialogService? DialogService { get; set; }

        [Inject]
        [NotNull]
        private UserApiServices? UserApiServices { get; set; }
        [Inject]
        [NotNull]
        private NavigationManager? NavigationManager { get; set; }
        [Inject]
        [NotNull]
        private MessageService? MessageService { get; set; }
        [Inject]
        private ILocalStorageService localStorage { get; set; }
        [Inject]
        public UserApiServices userApiServices { get; set; }

        private LoginModel Model { get; set; } = new LoginModel();

        public class LoginModel
        {
            [Required(ErrorMessage = "请输入账号")]
            public string Account { get; set; } = string.Empty;

            [Required(ErrorMessage = "请输入密码")]
            public string Password { get; set; } = string.Empty;
        }

        private async Task LoginClick(EditContext context)
        {
            try
            {
                var result = await UserApiServices.Login(Model.Account, Model.Password);
                if (result?.Succeeded == true)
                {
                    var userName = result.Data.Value<string>("name");
                    var account = result.Data.Value<string>("account");
                    localStorage.SetItemAsync("userName", userName);
                    localStorage.SetItemAsync("account", account);
                    NavigationManager.NavigateTo("/dashboard");
                }
                else
                {
                    await MessageService.Show(new MessageOption()
                    {
                        Content = result?.Message ?? "未知错误",
                        Icon = "fa - light fa - xmark",
                        Color = Color.Warning
                    });
                }
            }
            catch (Exception ex)
            {
                await MessageService.Show(new MessageOption()
                {
                    Content = "内部异常",
                    Icon = "fa - light fa - xmark",
                    Color = Color.Warning
                });
            }
        }

        public async Task ShowDialog()
        {
            var option = new ResultDialogOption
            {
                Title = "上传授权文件",
                Size = Size.Medium,
                ShowCloseButton = true,
                ShowFooter = false
            };

            await DialogService.ShowModal<AuthDialog>(option);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var result = await userApiServices.CheckLogin();
            if (result?.Succeeded == true)
            {
                var userName = result.Data.Value<string>("name");
                var account = result.Data.Value<string>("account");
                localStorage.SetItemAsync("userName", userName);
                localStorage.SetItemAsync("account", account);
                NavigationManager.NavigateTo("/dashboard");
            }
        }
    }
}
