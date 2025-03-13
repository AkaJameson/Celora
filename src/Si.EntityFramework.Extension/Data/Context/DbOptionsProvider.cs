using Si.EntityFramework.Extension.Data.Configurations;
using System.Collections.Concurrent;

namespace Si.EntityFramework.Extension.Data.Context
{
    public class DbOptionsProvider
    {
        private ConcurrentDictionary<string, DbOptions> OptionsDict = new ConcurrentDictionary<string, DbOptions>();
        public DbOptions this[string key]
        {
            get
            {
                if (OptionsDict.TryGetValue(key, out var options))
                {
                    return options;
                }
                return null;
            }
            set
            {
                OptionsDict.GetOrAdd(key, _ = value);
            }
        }


    }
}
