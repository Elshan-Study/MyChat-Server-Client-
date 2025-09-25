using System.Windows.Input;
using ChatClient.Helpers;

namespace ChatClient.ViewModels;
public class LoginViewModel
{
    public string Username { get; set; } = "User";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 7777;

    public ICommand ConnectCommand { get; }

    public event Func<string, string, int, Task>? ConnectRequested;

    public LoginViewModel()
    {
        ConnectCommand = new RelayCommand(async () =>
        {
            if (ConnectRequested != null)
                await ConnectRequested(Username, Host, Port);
        });
    }
}
