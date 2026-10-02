using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace MovieMogul.Web.Services;

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = false };

    public static void SetObject<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value, JsonOptions));

    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}
