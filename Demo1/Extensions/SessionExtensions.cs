using System.Text.Json;

namespace Demo1.Extensions;

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static void SetJson<T>(this ISession session, string key, T value)
        => session.SetString(key, JsonSerializer.Serialize(value, Options));

    public static T? GetJson<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException)
        {
            session.Remove(key);
            return default;
        }
    }
}
