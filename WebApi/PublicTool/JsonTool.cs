using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Model;
using System.IO;
using System.Collections.Generic;

namespace PublicTool
{
    /// <summary>
    /// 封装了JsonConvert
    /// </summary>
    public class JsonTool
    {

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(OperateResultModel srm)
        {
            if (srm.ReturnData == null)
            {
                srm.ReturnData = new object();
            }
            return SerializeObject((object)srm);
        }

        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o)
        {
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            //timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
            var jSetting = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",// "d MMMM, yyyy HH:mm:ss",
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(o, Formatting.Indented, jSetting);
            //json=JsonConvert.SerializeObject(o,Formatting.Indented, timeConverter);
            return json;
        }


        /// <summary>
        /// 将对象序列化为JSON格式
        /// </summary>
        /// <param name="o">对象</param>
        /// <param name="format">Indented or None(default:Indented)</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object o, string format)
        {
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            //timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
            var jSetting = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",// "d MMMM, yyyy HH:mm:ss",
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(o, "Indented".ToUpper().Equals(format.ToUpper()) ? Formatting.Indented : Formatting.None, jSetting);
            //json=JsonConvert.SerializeObject(o,Formatting.Indented, timeConverter);
            return json;
        }

        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
        /// <returns>对象实体</returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        /// <summary>
        /// 反序列化JSON到给定的匿名对象.
        /// </summary>
        /// <typeparam name="T">匿名对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <param name="anonymousTypeObject">匿名对象</param>
        /// <returns>匿名对象</returns>
        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
        {
            T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }

        public static string getDictionaryData(Dictionary<string, object> data)
        {
            string ret = null;
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value != null && item.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    ret += item.Key.ToString() + "={";
                    ret += getDictionaryData((Dictionary<string, object>)item.Value);
                    ret += "};";
                }
                else
                {
                    ret += item.Key.ToString() + "=" + (item.Value == null ? "null" : item.Value.ToString()) + ";";
                }
            }
            return ret;
        }

    }
}
