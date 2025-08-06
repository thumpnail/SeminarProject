namespace Chat.Common;

public static class JsonExtensions {
    public static string ToJson<T>(this T obj) {
        return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
    }
    public static T? FromJson<T>(this string json) {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }
}