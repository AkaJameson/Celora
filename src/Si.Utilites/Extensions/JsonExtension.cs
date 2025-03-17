using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Si.CoreHub.Extension
{
    public static class JsonExtensions
    {
        /// <summary>
        /// 提供简单的对象序列化Json字符串方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj) where T : class
        {
            var jSetting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, jSetting);
        }
        /// <summary>
        /// 提供简单的对象序列化Json字符串方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonEnumerable<T>(this T obj) where T : class
        {
            var arr = new List<T>();
            arr.Add(obj);
            var jSetting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(arr, jSetting);

        }

        /// <summary>
        /// 提供简单的对象Json字符串反序列化对象的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>___
        /// <returns></returns>
        public static T FromJson<T>(this string obj) where T : class
        {
            var jSetting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            if (!string.IsNullOrEmpty(obj))
            {
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;

        }


        /// <summary>
        /// 格式化Json字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertJsonString(this string str)
        {
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }
}
