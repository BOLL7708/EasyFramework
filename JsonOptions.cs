using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyFramework;

public static class JsonOptions
{
    private static readonly JsonSerializerOptions Instance = new()
    {
        IncludeFields = true, 
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static bool _initDone;

    public static JsonSerializerOptions Get()
    {
        if (_initDone) return Instance;
        
        Instance.Converters.Add(new JsonStringEnumConverter());
        _initDone = true;

        return Instance;
    }
}