using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyFramework;

public static class JsonUtils
{
    private static readonly JsonSerializerOptions Instance = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static bool _initDone;

    public static JsonSerializerOptions GetOptions()
    {
        if (_initDone) return Instance;

        Instance.Converters.Add(new JsonStringEnumConverter());
        _initDone = true;

        return Instance;
    }

    public class JsonDataParseResult<T>(T? data, T empty, string message)
    {
        public T? Data = data;
        public T Empty = empty;
        public string Message = message;
    }
    
    public static JsonDataParseResult<T> ParseData<T>(string? dataStr) where T : class
    {
        dataStr ??= "";
        T? data = null;
        var errorMessage = "";
        try
        {
            data = JsonSerializer.Deserialize<T>(dataStr, GetOptions());
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }
        return new JsonDataParseResult<T>(data, default!, errorMessage);
    }
}