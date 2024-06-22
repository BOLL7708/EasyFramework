using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyFramework;

public static class JsonOptions
{
    private static readonly JsonSerializerOptions Instance = new()
    {
        IncludeFields = true, 
        PropertyNameCaseInsensitive = true
    };
    private static bool _initDone;

    public static JsonSerializerOptions Get()
    {
        if (_initDone) return Instance;
        
        Instance.Converters.Add(new JsonStringEnumConverter());
        Instance.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        _initDone = true;

        return Instance;
    }
}