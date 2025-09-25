namespace MyChat;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class ChatServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<Guid, ClientHandler> _clients = new();

    public ChatServer(IPAddress ip, int port)
    {
        _listener = new TcpListener(ip, port);
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        _listener.Start();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var tcp = await _listener.AcceptTcpClientAsync(ct);
                var handler = new ClientHandler(tcp, this);
                _clients.TryAdd(handler.Id, handler);
                _ = handler.ProcessAsync(); 
            }
        }
        finally
        {
            _listener.Stop();
        }
    }
    
    internal async Task BroadcastUserListAsync()
    {
        var users = new List<string>();
        foreach (var h in _clients.Values)
            if (!string.IsNullOrEmpty(h.Username)) users.Add(h.Username);

        var packet = new Packet { Type = "UserList", Users = users };
        await BroadcastAsync(packet);
    }

    internal async Task BroadcastAsync(Packet packet)
    {
        var json = JsonSerializer.Serialize(packet);
        var tasks = new List<Task>();
        foreach (var client in _clients.Values)
        {
            tasks.Add(client.SendAsync(json));
        }
        await Task.WhenAll(tasks);
    }

    internal void RemoveClient(Guid id)
    {
        _clients.TryRemove(id, out _);
    }
}
