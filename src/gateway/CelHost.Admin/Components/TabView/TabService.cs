namespace CelHost.Admin.Components.TabView
{
    public class TabItem
    {
        public string Title { get; set; }
        public string Herf { get; set; }
        public Type ComponentType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IsSelected { get; set; } = false;
    }
    public class TabService : ITabService
    {
        public Dictionary<string, TabItem> Tabs { get; }
        public TabItem? ActiveTab { get; set; }
        /// <summary>
        /// 标签页切换后事件
        /// </summary>
        public event Action? OnTabChanged;
        public event Action? OnTabClose;

        public void CloseTab(string herf)
        {

            if (Tabs.TryGetValue(herf, out var tab))
            {
                Tabs.Remove(herf);
                OnTabClose?.Invoke();
            }
        }
        public void OpenTab(string title, string herf, Type componentType, Dictionary<string, object>? parameters = null)
        {
            if (!Tabs.TryGetValue(herf, out var tab))
            {
                var newTab = new TabItem
                {
                    Title = title,
                    Herf = herf,
                    ComponentType = componentType,
                    Parameters = parameters
                };
                Tabs.Add(herf, newTab);
                SetActive(newTab);
                OnTabChanged?.Invoke();
            }
        }

        public void SetActive(TabItem tab)
        {
            if (ActiveTab != null)
            {
                ActiveTab.IsSelected = false;
            }
            tab.IsSelected = true;
            ActiveTab = tab;
            OnTabChanged?.Invoke();
        }
    }
}
