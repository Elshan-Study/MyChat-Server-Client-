namespace ChatClient.Models;

public class ChatMessage
{
    public string Username { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public DateTime Time { get; init; } = DateTime.Now;
}