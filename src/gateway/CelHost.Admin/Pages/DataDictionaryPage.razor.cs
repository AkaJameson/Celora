using BootstrapBlazor.Components;
using CelHost.Apis.ApiServices;
using CelHost.Models.SystemDictModels;
using Microsoft.AspNetCore.Components;
using CelHost.Apis.Models;
using Microsoft.AspNetCore.Components.Web;
namespace CelHost.Admin.Pages
{
    public partial class DataDictionaryPage
    {
        [Inject]
        private DataDictionaryApiServices api { get; set; }
        private MessageService MessageService { get; set; }
        private DictAdd dictAdd { get; set; }
        protected void ShowAddDialog(MouseEventArgs e)
        {

        }
        protected async Task AddItem()
        {
            if (dictAdd == null)
            {
                return;
            }
            var result = await api.AddItem(dictAdd);
            if (result.Code == 200)
            {
                await MessageService.Show(new MessageOption
                {
                    Color = Color.Info,
                    Content = "添加成功"
                });
            }
            else
            {
                await MessageService.Show(new MessageOption
                {
                    Color = Color.Danger,
                    Content = result.Message
                });
            }
        }
        protected async Task<QueryData<DictItem>> QueryItems(QueryPageOptions options)
        {
            var result = await api.GetDataDictionary(new DictQuery
            {
                PageIndex = options.PageIndex,
                PageSize = options.PageItems
            });
            if (result.Code == 200)
            {
                return new QueryData<DictItem>
                {
                    Items = result.Data.Items,
                    TotalCount = result.Data.TotalCount
                };
            }
            else
            {
                return new QueryData<DictItem>()
                {
                    Items = new List<DictItem>(),
                    TotalCount = 0
                };
            }
        }
        protected async Task<IEnumerable<TableTreeNode<DictItem>>> TreeNodeConverter(IEnumerable<DictItem> dataItems)
        {
            return await Task.FromResult(ConvertToTreeNodes(dataItems));
        }

        private IEnumerable<TableTreeNode<DictItem>> ConvertToTreeNodes(IEnumerable<DictItem> items, TableTreeNode<DictItem>? parent = null)
        {
            var nodes = new List<TableTreeNode<DictItem>>();

            foreach (var item in items)
            {
                var node = new TableTreeNode<DictItem>(item)
                {
                    Parent = parent,
                    Items = ConvertToTreeNodes(item.Child, null),
                    HasChildren = item.Child?.Any() == true
                };

                nodes.Add(node);
            }

            return nodes;
        }

        public Task<IEnumerable<TableTreeNode<DictItem>>> OnTreeExpand(DictItem item)
        {
            return Task.FromResult(ConvertToTreeNodes(item.Child));
        }

    }

}
