using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wtrfll.Server.Tests.Common;

public static class TestJson
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}
