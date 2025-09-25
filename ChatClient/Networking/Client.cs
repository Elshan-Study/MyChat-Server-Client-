namespace ChatClient;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class Client : IDisposable
{
    private readonly TcpClient _tcp = new();
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    public event Action<Packet>? PacketReceived;
    public event Action? Disconnected;

    public bool IsConnected => _tcp?.Connected ?? false;

    public async Task ConnectAsync(string host, int port, CancellationToken ct = default)
    {
        await _tcp.ConnectAsync(host, port, ct);
        _stream = _tcp.GetStream();
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    public async Task SendAsync(Packet packet)
    {
        if (_stream == null) return;
        var json = JsonSerializer.Serialize(packet);
        var payload = Encoding.UTF8.GetBytes(json);
        var len = BitConverter.GetBytes(payload.Length);
        await _stream.WriteAsync(len);
        await _stream.WriteAsync(payload);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var json = await ReadPacketAsync(_stream!);
                if (json == null) break;
                var packet = JsonSerializer.Deserialize<Packet>(json);
                if (packet != null)
                    PacketReceived?.Invoke(packet);
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            Disconnected?.Invoke();
            Dispose();
        }
    }

    private static async Task<string?> ReadPacketAsync(NetworkStream stream)
    {
        var lenBuf = new byte[4];
        var got = 0;
        while (got < 4)
        {
            var r = await stream.ReadAsync(lenBuf.AsMemory(got, 4 - got));
            if (r == 0) return null;
            got += r;
        }
        var len = BitConverter.ToInt32(lenBuf, 0);
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

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _stream?.Dispose(); } catch { }
        try { _tcp?.Close(); } catch { }
    }
}
