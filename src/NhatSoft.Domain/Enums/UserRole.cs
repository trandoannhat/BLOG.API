using System.Text.Json.Serialization; 

namespace NhatSoft.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum UserRole
{
    Admin = 0,
    Editor = 1,
    Client = 2
}