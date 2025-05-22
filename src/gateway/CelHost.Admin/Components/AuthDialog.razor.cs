using BootstrapBlazor.Components;
using CelHost.Apis.ApiServices;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace CelHost.Admin.Components
{
    public partial class AuthDialog: IResultDialog
    {
        [Inject]
        [NotNull]
        private UserApiServices? UserApiServices { get; set; }

        [Inject]
        [NotNull]
        private MessageService? MessageService { get; set; }

        private bool IsVisible { get; set; }
        private bool IsUploading { get; set; } = false;
        private UploadFile? File { get; set; }

        // 实现 IResultDialog 的 OnClose 方法
        public async Task OnClose(DialogResult result)
        {
            IsVisible = false; // 标记为关闭

            StateHasChanged();
        }

        // 显示弹窗（供外部调用）
        public void Show()
        {
            IsVisible = true;
            StateHasChanged();
        }

        private Task OnFileChange(UploadFile file)
        {
            File = file;
            return Task.CompletedTask;
        }

        private Task<bool> OnFileDelete(UploadFile file)
        {
            File = null;
            return Task.FromResult(true);
        }

        private bool CanUpload => File != null && !IsUploading;

        private async Task UploadFile()
        {
            if (File?.File == null)
            {
                await MessageService.Show(new MessageOption()
                {
                    Content = "未上传文件",
                    Icon = "fa-solid fa-circle-info",
                    Color = Color.Warning
                });
                return;
            }

            IsUploading = true;
            try
            {
                var result = await UserApiServices.InitUser(File.File);
                if (result == null || result.Code != 200)
                {
                    await MessageService.Show(new MessageOption()
                    {
                        Content = result?.Message ?? "授权失败，请联系管理员",
                        Icon = "fa-solid fa-circle-info",
                        Color = Color.Danger
                    });
                }
                else
                {
                    await MessageService.Show(new MessageOption()
                    {
                        Content = "授权成功",
                        Icon = "fa-solid fa-circle-info",
                        Color = Color.Success
                    });
                    await OnClose(DialogResult.Yes);
                }
            }
            catch (Exception ex)
            {
                await MessageService.Show(new MessageOption()
                {
                    Content = $"授权失败：{ex.Message}",
                    Icon = "fa-solid fa-circle-info",
                    Color = Color.Danger
                });
            }
            finally
            {
                IsUploading = false;
            }
        }
    }
}
