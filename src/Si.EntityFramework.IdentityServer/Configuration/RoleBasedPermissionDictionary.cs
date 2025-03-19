using System.Collections;
using System.Collections.Concurrent;

namespace Si.EntityFramework.IdentityServer.Configuration
{
    public class RoleBasedPermissionDictionary : IDictionary<string, List<string>>
    {
        private readonly ConcurrentDictionary<string, List<string>> _dictionary;

        private static Lazy<RoleBasedPermissionDictionary> _lazyInstance = new Lazy<RoleBasedPermissionDictionary>(() => new RoleBasedPermissionDictionary());
        public static RoleBasedPermissionDictionary _instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }
        public RoleBasedPermissionDictionary()
        {
            _dictionary = new ConcurrentDictionary<string, List<string>>();
        }

        public List<string> this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<string> Keys => _dictionary.Keys;
        public ICollection<List<string>> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public void Add(string key, List<string> value) => _dictionary.TryAdd(key, value);
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);
        public bool Remove(string key) => _dictionary.TryRemove(key, out _);
        public bool TryGetValue(string key, out List<string> value) => _dictionary.TryGetValue(key, out value);

        public void Add(KeyValuePair<string, List<string>> item) => _dictionary.TryAdd(item.Key, item.Value);
        public void Clear() => _dictionary.Clear();
        public bool Contains(KeyValuePair<string, List<string>> item) => _dictionary.TryGetValue(item.Key, out var value) && EqualityComparer<List<string>>.Default.Equals(value, item.Value);
        public void CopyTo(KeyValuePair<string, List<string>>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<string, List<string>> item) => _dictionary.TryRemove(item.Key, out _);

        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
