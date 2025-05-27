namespace CelHost.Admin.Components.TabView
{
    public interface ITabService
    {
        public Dictionary<string, TabItem> Tabs { get; }
        TabItem? ActiveTab { get; set; }
        event Action? OnTabChanged;
        event Action? OnTabClose;
        void CloseTab(string herf);
        void OpenTab(string title, string herf, Type componentType, Dictionary<string, object>? parameters = null);
        void SetActive(TabItem tab);
    }
}
