using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Si.Modules.EventBus
{
    public class EventData : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _data = new();
        public object this[string key] { get => _data[key]; set => _data[key] = value; }
        public ICollection<string> Keys => _data.Keys;
        public ICollection<object> Values => _data.Values;
        public int Count => _data.Count;
        public bool IsReadOnly => false;

        public void Add(string key, object value) => _data.Add(key, value);
        public void Add(KeyValuePair<string, object> item) => _data.Add(item.Key, item.Value);
        public void Clear() => _data.Clear();
        public bool Contains(KeyValuePair<string, object> item) => _data.ContainsKey(item.Key) && _data[item.Key].Equals(item.Value);
        public bool ContainsKey(string key) => _data.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((IDictionary<string, object>)_data).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _data.GetEnumerator();
        public bool Remove(string key) => _data.Remove(key);
        public bool Remove(KeyValuePair<string, object> item) => _data.Remove(item.Key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _data.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    }
}
