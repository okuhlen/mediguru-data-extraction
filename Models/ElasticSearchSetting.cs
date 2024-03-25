using System.Text.Json.Serialization;

namespace MediGuru.DataExtractionTool.Models;

public sealed class ElasticSearchSetting
{
    [JsonPropertyName("host")]
    public string Host { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("port")]
    public string Port { get; set; }
}