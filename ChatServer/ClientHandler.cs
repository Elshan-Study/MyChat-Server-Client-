using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyChat;

public class ClientHandler
{
    public Guid Id { get; } = Guid.NewGuid();
    private readonly TcpClient _tcp;
    private readonly ChatServer _server;
    private readonly NetworkStream _stream;
    public string Username { get; private set; } = string.Empty;

    public ClientHandler(TcpClient tcp, ChatServer server)
    {
        _tcp = tcp;
        _server = server;
        _stream = tcp.GetStream();
    }

    public async Task ProcessAsync()
    {
        try
        {
            Console.WriteLine($"Handler {Id} started. Remote: {_tcp.Client.RemoteEndPoint}");

            while (true)
            {
                var json = await ReadPacketAsync(_stream);
                Console.WriteLine($"Handler {Id} ReadPacketAsync => {(json == null ? "null" : $"len={json.Length}")}");
                if (json == null) break;

                Console.WriteLine($"Handler {Id} RAW JSON: {json}");

                Packet? packet = null;
                try
                {
                    packet = JsonSerializer.Deserialize<Packet>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Handler {Id} JSON deserialize error: {ex}");
                    continue;
                }

                if (packet == null)
                {
                    Console.WriteLine($"Handler {Id} packet == null");
                    continue;
                }

                Console.WriteLine($"Handler {Id} Packet.Type = {packet.Type}");

                switch (packet.Type)
                {
                    case "Join":
                        Username = packet.Username ?? string.Empty;
                        Console.WriteLine($"Handler {Id} Username set to: '{Username}'");
                        await _server.BroadcastUserListAsync();
                        Console.WriteLine($"{Username} joined the chat.");
                        await _server.BroadcastAsync(new Packet { Type = "Message", Username = "System", Text = $"{Username} joined the chat." });
                        break;

                    case "Message":
                        var broadcast = new Packet { Type = "Message", Username = Username, Text = packet.Text };
                        Console.WriteLine($"Broadcasting message from {Username}: {packet.Text}");
                        await _server.BroadcastAsync(broadcast);
                        break;

                    default:
                        Console.WriteLine($"Unknown packet type: {packet.Type}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in handler {Id}: {ex}");
        }
        finally
        {
            _server.RemoveClient(Id);
            await _server.BroadcastUserListAsync();

            if (!string.IsNullOrEmpty(Username))
            {
                Console.WriteLine($"{Username} left the chat.");
                await _server.BroadcastAsync(new Packet { Type = "Message", Username = "System", Text = $"{Username} left the chat." });
            }

            _tcp.Dispose();
        }
    }

    // отправка JSON-строки (length-prefixed)
    public async Task SendAsync(string json)
    {
        try
        {
            var payload = Encoding.UTF8.GetBytes(json);
            var len = BitConverter.GetBytes(payload.Length);
            await _stream.WriteAsync(len);
            await _stream.WriteAsync(payload);
        }
        catch
        {
            //...
        }
    }
    
    private static async Task<string?> ReadPacketAsync(NetworkStream stream)
    {
        var lenBuffer = new byte[4];
        var got = 0;
        while (got < 4)
        {
            var r = await stream.ReadAsync(lenBuffer.AsMemory(got, 4 - got));
            if (r == 0) return null; 
            got += r;
        }
        var len = BitConverter.ToInt32(lenBuffer, 0);
        if (len <= 0) return null;
        var payload = new byte[len];
        got = 0;
        while (got < len)
        {
            var r = await stream.ReadAsync(payload.AsMemory(got, len - got));
            if (r == 0) return null;
            got += r;
        }
        return Encoding.UTF8.GetString(payload);
    }
}
