using System.Configuration;
using System.Data;
using System.Windows;
using ChatClient.ViewModels;

namespace ChatClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var loginVm = new LoginViewModel();
        var loginWindow = new LoginWindow { DataContext = loginVm };

        loginVm.ConnectRequested += async (username, host, port) =>
        {
            var client = new Client();
            try
            {
                await client.ConnectAsync(host, port);
                // отправляем Join
                await client.SendAsync(new Packet { Type = "Join", Username = username });

                // открываем главное окно
                var mainVm = new MainViewModel(client, username);
                var mainWindow = new MainWindow { DataContext = mainVm };
                mainWindow.Show();
                loginWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect: " + ex.Message);
            }
        };

        loginWindow.Show();
    }
}