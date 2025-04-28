using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;
using System.Dynamic;

namespace CelHost.Fronter.Utils
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            Converters = new List<Newtonsoft.Json.JsonConverter>(),
            Error = (sender, args) =>
            {
                // 可以自定义错误处理逻辑
                args.ErrorContext.Handled = true;
            }
        };

        // 序列化对象为 JSON 字符串
        public static string ToJson<T>(this T obj, bool indented = false)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var settings = indented ? new JsonSerializerSettings
            {
                ContractResolver = _defaultSettings.ContractResolver,
                MissingMemberHandling = _defaultSettings.MissingMemberHandling,
                NullValueHandling = _defaultSettings.NullValueHandling,
                DateFormatHandling = _defaultSettings.DateFormatHandling,
                DateParseHandling = _defaultSettings.DateParseHandling,
                Formatting = Formatting.Indented,
                MetadataPropertyHandling = _defaultSettings.MetadataPropertyHandling,
                Converters = _defaultSettings.Converters,
                Error = _defaultSettings.Error
            } : _defaultSettings;

            try
            {
                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (JsonSerializationException ex)
            {
                throw new JsonException($"Error serializing object to JSON: {ex.Message}", ex);
            }
        }

        // 反序列化 JSON 字符串为指定类型
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));

            try
            {
                return JsonConvert.DeserializeObject<T>(json, _defaultSettings)
                    ?? throw new JsonException($"Failed to deserialize JSON to type {typeof(T).Name}: Result is null");
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error deserializing JSON to type {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        // 反序列化 JSON 字符串为动态 JToken
        public static JToken ToJsonDynamic(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));

            try
            {
                return JToken.Parse(json);
            }
            catch (JsonReaderException ex)
            {
                throw new JsonException($"Invalid JSON format: {ex.Message}", ex);
            }
        }

        // 反序列化 JSON 字符串为动态对象 (ExpandoObject)
        public static dynamic ToDynamicObject(this string json)
        {
            var jToken = json.ToJsonDynamic();
            return ToExpandoObject(jToken);
        }

        // 动态访问 JToken 属性
        public static object? GetDynamicValue(this JToken token, string propertyName)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var value = token[propertyName];
            if (value == null)
                return null;

            try
            {
                return value.Type switch
                {
                    JTokenType.String => value.ToString(),
                    JTokenType.Integer => value.ToObject<long>(),
                    JTokenType.Float => value.ToObject<double>(),
                    JTokenType.Boolean => value.ToObject<bool>(),
                    JTokenType.Null => null,
                    JTokenType.Object => ToExpandoObject(value),
                    JTokenType.Array => value.ToObject<object[]>(),
                    JTokenType.Date => value.ToObject<DateTime>(),
                    _ => value.ToString()
                };
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error accessing property '{propertyName}': {ex.Message}", ex);
            }
        }

        // 获取嵌套属性值
        public static JToken GetNestedProperty(this JToken token, params string[] propertyPath)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var current = token;
            foreach (var prop in propertyPath)
            {
                current = current[prop];
                if (current == null)
                    throw new JsonException($"Property '{prop}' not found in JSON object.");
            }
            return current;
        }

        // 辅助方法：将 JToken 转换为 ExpandoObject
        private static dynamic ToExpandoObject(JToken token)
        {
            if (token is JObject jObject)
            {
                var expando = new ExpandoObject() as IDictionary<string, object?>;
                foreach (var property in jObject.Properties())
                {
                    expando[property.Name] = ToExpandoObject(property.Value);
                }
                return expando;
            }
            else if (token is JArray jArray)
            {
                var list = new List<object?>();
                foreach (var item in jArray)
                {
                    list.Add(ToExpandoObject(item));
                }
                return list;
            }
            else
            {
                return ((JValue)token).Value;
            }
        }
    }

}
