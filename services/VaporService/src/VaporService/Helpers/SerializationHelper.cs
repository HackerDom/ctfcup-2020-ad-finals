using System.Text;
using Newtonsoft.Json;

namespace VaporService.Helpers
{
    internal static class SerializationHelper
    {
        public static TValue FromJson<TValue>(this string source)
        {
            return JsonConvert.DeserializeObject<TValue>(source);
        }

        public static string ToJson<TValue>(this TValue source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static TValue FromBytes<TValue>(this byte[] source)
        {
            return Encoding.UTF8.GetString(source).FromJson<TValue>();
        }

        public static byte[] ToBytes<TValue>(this TValue source)
        {
            return Encoding.UTF8.GetBytes(source.ToJson());
        }
    }
}