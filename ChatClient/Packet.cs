namespace ChatClient;
using System.Collections.Generic;

public record Packet
{
    // Type: "Join", "Leave", "UserList", "Message"
    public string Type { get; init; } = string.Empty;

    // От кого сообщение (для Type="Join" / "Message")
    public string Username { get; init; } = string.Empty;

    // Текст сообщения (для Type="Message")
    public string? Text { get; init; }

    // Список пользователей (для Type="UserList")
    public List<string>? Users { get; init; }
}
