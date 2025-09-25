using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ChatClient.Helpers;
using ChatClient.Models;

namespace ChatClient.ViewModels;

public class MainViewModel : BaseNotify
{
    private readonly Client _client;
    public ObservableCollection<string> Users { get; } = new();
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    private string _messageText = string.Empty;
    public string MessageText
    {
        get => _messageText;
        set { _messageText = value; OnPropertyChanged(); SendCommand_Relay.RaiseCanExecuteChanged(); }
    }

    public RelayCommand SendCommand_Relay { get; }
    public ICommand SendCommand => SendCommand_Relay;

    public string Username { get; }

    public MainViewModel(Client client, string username)
    {
        _client = client;
        Username = username;

        SendCommand_Relay = new RelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(MessageText)) return;
            var packet = new Packet { Type = "Message", Text = MessageText };
            await _client.SendAsync(packet);
            MessageText = string.Empty;
        }, () => !string.IsNullOrWhiteSpace(MessageText));

        _client.PacketReceived += OnPacketReceived;
        _client.Disconnected += OnDisconnected;
    }

    private void OnDisconnected()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Messages.Add(new ChatMessage { Username = "System", Text = "Disconnected from server." });
        });
    }

    private void OnPacketReceived(Packet p)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (p.Type)
            {
                case "UserList":
                    Users.Clear();
                    if (p.Users != null)
                        foreach (var u in p.Users)
                            Users.Add(u);
                    break;
                case "Message":
                    Messages.Add(new ChatMessage { Username = p.Username ?? "?", Text = p.Text ?? "" });
                    break;
            }
        });
    }
}